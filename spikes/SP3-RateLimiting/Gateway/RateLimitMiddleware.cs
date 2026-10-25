using Microsoft.AspNetCore.Mvc;

namespace ShopStream.Spike.Gateway;

/// <summary>
/// Classifies a request into the D4 limit classes and applies them cheapest-rejection-first.
/// Spike simplification: the user id comes from the X-Spike-User header. In the real gateway it comes from the
/// validated session or token (ADR-008). X-Forwarded-For is trusted here; production must only trust the edge proxy.
/// </summary>
public sealed class RateLimitMiddleware(RequestDelegate next, AdmissionLimiter limiter, LimiterOptions options, ILogger<RateLimitMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? "/";
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)) { await next(ctx); return; }

        var user = ctx.Request.Headers["X-Spike-User"].FirstOrDefault();
        var ip = ClientIp(ctx);
        var isOrder = HttpMethods.IsPost(ctx.Request.Method) && path.TrimEnd('/').Equals("/api/v1/orders", StringComparison.OrdinalIgnoreCase);
        ctx.Response.Headers["X-Gateway-Instance"] = options.InstanceName;

        if (path.StartsWith("/connect/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/auth/", StringComparison.OrdinalIgnoreCase))
        {
            if (!await Check(ctx, LimitPolicies.Login, ip)) return;
            await next(ctx); return;
        }

        if (isOrder && user is not null)
        {
            if (!await Check(ctx, LimitPolicies.PlaceOrderRate, user)) return;
            if (!await Check(ctx, LimitPolicies.CheckoutAdmission, "all", "high-demand")) return;

            var started = System.Diagnostics.Stopwatch.GetTimestamp();
            var inflight = await limiter.AcquireAsync(LimitPolicies.PlaceOrderInFlight, user);
            LogIfSlow(LimitPolicies.PlaceOrderInFlight, inflight, started);
            if (!inflight.Allowed) { await Reject(ctx, LimitPolicies.PlaceOrderInFlight, inflight, "duplicate-in-flight"); return; }
            try { Tag(ctx, inflight); await next(ctx); }
            finally { await limiter.ReleaseAsync(LimitPolicies.PlaceOrderInFlight, user, inflight.Source); }
            return;
        }

        var (policy, key) =
            path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) && user is not null ? (LimitPolicies.Staff, user) :
            user is not null ? (LimitPolicies.AuthenticatedApi, user) :
            (LimitPolicies.AnonymousBrowse, ip);

        if (!await Check(ctx, policy, key)) return;
        await next(ctx);
    }

    private async Task<bool> Check(HttpContext ctx, LimitPolicy policy, string key, string problem = "rate-limited")
    {
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var d = await limiter.AcquireAsync(policy, key);
        LogIfSlow(policy, d, started);
        Tag(ctx, d);
        if (d.Allowed) return true;
        await Reject(ctx, policy, d, problem);
        return false;
    }

    private void LogIfSlow(LimitPolicy policy, Decision d, long startedTimestamp)
    {
        var ms = System.Diagnostics.Stopwatch.GetElapsedTime(startedTimestamp).TotalMilliseconds;
        if (ms > 100)
            log.LogWarning("Slow limit decision: {Ms:F0} ms for {Policy} via {Source}, store considered up: {Up}",
                ms, policy.Name, d.Source, limiter.StoreConsideredUp);
    }

    private static void Tag(HttpContext ctx, Decision d) => ctx.Response.Headers["X-RateLimit-Source"] = d.Source.ToString();

    private static async Task Reject(HttpContext ctx, LimitPolicy policy, Decision d, string problem)
    {
        Tag(ctx, d);
        var seconds = Math.Max(1, (int)Math.Ceiling(d.RetryAfter.TotalSeconds));
        ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.Response.Headers.RetryAfter = seconds.ToString();
        ctx.Response.ContentType = "application/problem+json";
        var body = new ProblemDetails
        {
            Status = 429,
            Title = problem == "high-demand" ? "High demand, please retry" : "Too many requests",
            Type = $"https://shopstream.example/problems/{problem}",
            Detail = $"Limit class '{policy.Name}' exceeded. Retry after {seconds}s."
        };
        await ctx.Response.WriteAsJsonAsync(body, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
    }

    private static string ClientIp(HttpContext ctx)
    {
        var xff = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xff)) return xff.Split(',')[0].Trim();
        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
