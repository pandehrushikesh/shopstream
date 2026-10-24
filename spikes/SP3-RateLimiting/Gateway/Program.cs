using ShopStream.Spike.Gateway;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var options = builder.Configuration.GetSection("RateLimiting").Get<LimiterOptions>() ?? new LimiterOptions();
builder.Services.AddSingleton(options);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options.Redis));
builder.Services.AddSingleton<AdmissionLimiter>();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseMiddleware<RateLimitMiddleware>();

// Stand-ins so the spike runs without the real services.
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapPost("/api/v1/orders", async () => { await Task.Delay(50); return Results.Accepted(); });
app.MapGet("/spike/ping", () => Results.Ok(new { pong = true }));
app.MapReverseProxy();

app.Run();
