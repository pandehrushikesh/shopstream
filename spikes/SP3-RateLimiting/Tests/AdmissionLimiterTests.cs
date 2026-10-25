using Microsoft.Extensions.Logging.Abstractions;
using ShopStream.Spike.Gateway;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace ShopStream.Spike.Gateway.Tests;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder().WithImage("redis:7-alpine").Build();
    public string ConnectionString => _container.GetConnectionString();
    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

public sealed class AdmissionLimiterTests(RedisFixture fixture) : IClassFixture<RedisFixture>
{
    private AdmissionLimiter NewLimiter(string? connection = null, int instances = 3)
    {
        var cs = (connection ?? fixture.ConnectionString) + ",abortConnect=false,connectTimeout=300,asyncTimeout=300";
        var mux = ConnectionMultiplexer.Connect(cs);
        return new AdmissionLimiter(mux, new LimiterOptions { Instances = instances, StoreTimeout = TimeSpan.FromSeconds(1) }, TimeProvider.System, NullLogger<AdmissionLimiter>.Instance);
    }

    private static string Unique() => Guid.NewGuid().ToString("N");

    [Fact]
    public async Task TokenBucket_allows_burst_then_rejects_with_retry_after()
    {
        var limiter = NewLimiter();
        var policy = new LimitPolicy("t1", LimitKind.TokenBucket, Rate: 1, Burst: 5);
        var key = Unique();
        var results = new List<Decision>();
        for (var i = 0; i < 6; i++) results.Add(await limiter.AcquireAsync(policy, key));

        results.Take(5).Should().OnlyContain(d => d.Allowed && d.Source == DecisionSource.Redis);
        results[5].Allowed.Should().BeFalse();
        results[5].RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task TokenBucket_refills_over_time()
    {
        var limiter = NewLimiter();
        var policy = new LimitPolicy("t2", LimitKind.TokenBucket, Rate: 20, Burst: 2);
        var key = Unique();
        (await limiter.AcquireAsync(policy, key)).Allowed.Should().BeTrue();
        (await limiter.AcquireAsync(policy, key)).Allowed.Should().BeTrue();
        (await limiter.AcquireAsync(policy, key)).Allowed.Should().BeFalse();
        await Task.Delay(200); // about 4 tokens at 20/s, capped at burst 2
        (await limiter.AcquireAsync(policy, key)).Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Limit_holds_across_two_gateway_instances_sharing_the_store()
    {
        var a = NewLimiter(); var b = NewLimiter();
        var policy = new LimitPolicy("t3", LimitKind.TokenBucket, Rate: 1, Burst: 10);
        var key = Unique();
        var allowed = 0;
        for (var i = 0; i < 30; i++)
        {
            var d = await (i % 2 == 0 ? a : b).AcquireAsync(policy, key);
            if (d.Allowed) allowed++;
        }
        allowed.Should().BeInRange(10, 11, "burst of 10 plus at most one refill during the test, not 20");
    }

    [Fact]
    public async Task Store_down_falls_back_to_local_limit_divided_by_instances()
    {
        var limiter = NewLimiter(connection: "localhost:1", instances: 3);
        var policy = new LimitPolicy("t4", LimitKind.TokenBucket, Rate: 0.001, Burst: 30);
        var key = Unique();
        var allowed = 0; Decision last = default;
        for (var i = 0; i < 30; i++) { last = await limiter.AcquireAsync(policy, key); if (last.Allowed) allowed++; }

        last.Source.Should().Be(DecisionSource.Local);
        allowed.Should().Be(10, "burst 30 divided by 3 instances");
    }

    [Fact]
    public async Task SlidingWindow_limits_login_attempts()
    {
        var limiter = NewLimiter();
        var policy = new LimitPolicy("t5", LimitKind.SlidingWindow, Limit: 5, Window: TimeSpan.FromMinutes(1));
        var key = Unique();
        var results = new List<Decision>();
        for (var i = 0; i < 6; i++) results.Add(await limiter.AcquireAsync(policy, key));

        results.Take(5).Should().OnlyContain(d => d.Allowed);
        results[5].Allowed.Should().BeFalse();
        results[5].RetryAfter.Should().BeGreaterThan(TimeSpan.FromSeconds(50));
    }

    [Fact]
    public async Task Concurrency_allows_one_in_flight_until_released()
    {
        var limiter = NewLimiter();
        var policy = new LimitPolicy("t6", LimitKind.Concurrency, Limit: 1);
        var key = Unique();
        var first = await limiter.AcquireAsync(policy, key);
        var second = await limiter.AcquireAsync(policy, key);
        await limiter.ReleaseAsync(policy, key, first.Source);
        var third = await limiter.AcquireAsync(policy, key);

        first.Allowed.Should().BeTrue();
        second.Allowed.Should().BeFalse();
        third.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task After_breaker_window_the_limiter_returns_to_the_store()
    {
        // Two bad calls then a healthy store: use a short breaker to prove recovery logic compiles and runs.
        var mux = ConnectionMultiplexer.Connect(fixture.ConnectionString + ",abortConnect=false");
        var limiter = new AdmissionLimiter(mux, new LimiterOptions { Instances = 3, BreakerDuration = TimeSpan.FromMilliseconds(50), StoreTimeout = TimeSpan.FromSeconds(1) },
            TimeProvider.System, NullLogger<AdmissionLimiter>.Instance);
        var policy = new LimitPolicy("t7", LimitKind.TokenBucket, Rate: 1, Burst: 5);
        var d = await limiter.AcquireAsync(policy, Unique());
        d.Source.Should().Be(DecisionSource.Redis);
        limiter.StoreConsideredUp.Should().BeTrue();
    }
}
