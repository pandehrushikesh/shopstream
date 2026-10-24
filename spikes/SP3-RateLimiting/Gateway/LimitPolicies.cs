namespace ShopStream.Spike.Gateway;

public enum LimitKind { TokenBucket, SlidingWindow, Concurrency }

/// <summary>One row of the D4 section 4.2 limit table. Values are planning assumptions.</summary>
public sealed record LimitPolicy(
    string Name,
    LimitKind Kind,
    double Rate = 0,          // tokens per second (TokenBucket)
    int Burst = 0,            // bucket size (TokenBucket)
    int Limit = 0,            // events per window (SlidingWindow) or in-flight requests (Concurrency)
    TimeSpan Window = default);

public static class LimitPolicies
{
    public static readonly LimitPolicy AnonymousBrowse = new("anon-browse", LimitKind.TokenBucket, Rate: 20, Burst: 60);
    public static readonly LimitPolicy AuthenticatedApi = new("auth-api", LimitKind.TokenBucket, Rate: 10, Burst: 30);
    public static readonly LimitPolicy Login = new("login", LimitKind.SlidingWindow, Limit: 5, Window: TimeSpan.FromMinutes(1));
    public static readonly LimitPolicy PlaceOrderRate = new("place-order", LimitKind.TokenBucket, Rate: 0.5, Burst: 3);
    public static readonly LimitPolicy PlaceOrderInFlight = new("place-order-inflight", LimitKind.Concurrency, Limit: 1);
    public static readonly LimitPolicy CheckoutAdmission = new("checkout-admission", LimitKind.TokenBucket, Rate: 600, Burst: 100);
    public static readonly LimitPolicy Staff = new("staff", LimitKind.TokenBucket, Rate: 5, Burst: 15);
}
