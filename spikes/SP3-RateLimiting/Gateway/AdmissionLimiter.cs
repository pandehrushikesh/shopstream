using System.Collections.Concurrent;
using StackExchange.Redis;

namespace ShopStream.Spike.Gateway;

public enum DecisionSource { Redis, Local }

public readonly record struct Decision(bool Allowed, TimeSpan RetryAfter, DecisionSource Source);

public sealed class LimiterOptions
{
    /// <summary>Number of gateway instances. Used to divide limits when the shared store is down (D4 4.1).</summary>
    public int Instances { get; set; } = 3;
    public string InstanceName { get; set; } = "gw";
    public string Redis { get; set; } = "localhost:6379,abortConnect=false,connectTimeout=500,asyncTimeout=150";
    /// <summary>How long to stay on local limits after a store failure before trying the store again.</summary>
    public TimeSpan BreakerDuration { get; set; } = TimeSpan.FromSeconds(5);
    /// <summary>
    /// Hard deadline for one store call. The client library enforces its own timeout on a heartbeat of about one
    /// second, so a call on a connection that just died can hang for seconds. This deadline is ours and is exact.
    /// </summary>
    public TimeSpan StoreTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
}

/// <summary>
/// Shared limiter backed by atomic Lua scripts in a Redis-compatible store, with a safe local fallback.
/// Fallback rule (D4 4.1): local limit = global limit / number of instances. Fails safe, never open.
/// Spike code: local state is not evicted (production needs expiry) and scripts use EVAL, not EVALSHA.
/// </summary>
public sealed class AdmissionLimiter
{
    // KEYS[1]=bucket key. ARGV: rate per second, burst, cost. Time comes from the store to avoid clock skew between gateways.
    private const string TokenBucketLua = """
        local rate = tonumber(ARGV[1]); local burst = tonumber(ARGV[2]); local cost = tonumber(ARGV[3])
        local t = redis.call('TIME'); local now = t[1] * 1000 + math.floor(t[2] / 1000)
        local d = redis.call('HMGET', KEYS[1], 'tokens', 'ts')
        local tokens = tonumber(d[1]); local ts = tonumber(d[2])
        if tokens == nil then tokens = burst; ts = now end
        local elapsed = math.max(0, now - ts)
        tokens = math.min(burst, tokens + elapsed * rate / 1000)
        local allowed = 0; local retry = 0
        if tokens >= cost then tokens = tokens - cost; allowed = 1
        else retry = math.ceil((cost - tokens) / rate * 1000) end
        redis.call('HSET', KEYS[1], 'tokens', tostring(tokens), 'ts', now)
        redis.call('PEXPIRE', KEYS[1], math.ceil(burst / rate * 1000) * 2 + 1000)
        return {allowed, retry}
        """;

    // KEYS[1]=zset key. ARGV: window ms, limit, unique member suffix.
    private const string SlidingWindowLua = """
        local window = tonumber(ARGV[1]); local limit = tonumber(ARGV[2])
        local t = redis.call('TIME'); local now = t[1] * 1000 + math.floor(t[2] / 1000)
        redis.call('ZREMRANGEBYSCORE', KEYS[1], 0, now - window)
        local count = redis.call('ZCARD', KEYS[1])
        if count < limit then
          redis.call('ZADD', KEYS[1], now, now .. '-' .. ARGV[3])
          redis.call('PEXPIRE', KEYS[1], window)
          return {1, 0}
        end
        local oldest = redis.call('ZRANGE', KEYS[1], 0, 0, 'WITHSCORES')
        return {0, window - (now - tonumber(oldest[2]))}
        """;

    // KEYS[1]=counter key. ARGV: limit, ttl ms (guards against leaks if a release is lost).
    private const string ConcurrencyAcquireLua = """
        local c = redis.call('INCR', KEYS[1])
        redis.call('PEXPIRE', KEYS[1], ARGV[2])
        if c > tonumber(ARGV[1]) then redis.call('DECR', KEYS[1]); return {0, 0} end
        return {1, 0}
        """;

    private const string ConcurrencyReleaseLua = """
        local c = tonumber(redis.call('GET', KEYS[1]) or '0')
        if c > 0 then redis.call('DECR', KEYS[1]) end
        return c
        """;

    private readonly IConnectionMultiplexer _redis;
    private readonly LimiterOptions _options;
    private readonly TimeProvider _time;
    private readonly ILogger<AdmissionLimiter> _log;
    // Breaker state. 0 = store healthy. Otherwise the timestamp before which we stay on local limits.
    // After that time the breaker is half-open: exactly one request probes the store, everyone else stays local.
    private long _redisRetryAt;
    private int _probing;

    private readonly ConcurrentDictionary<string, LocalBucket> _buckets = new();
    private readonly ConcurrentDictionary<string, LocalWindow> _windows = new();
    private readonly ConcurrentDictionary<string, LocalCounter> _counters = new();

    public AdmissionLimiter(IConnectionMultiplexer redis, LimiterOptions options, TimeProvider time, ILogger<AdmissionLimiter> log)
    {
        _redis = redis; _options = options; _time = time; _log = log;
    }

    public bool StoreConsideredUp
    {
        get { var r = Volatile.Read(ref _redisRetryAt); return r == 0 || _time.GetTimestamp() >= r; }
    }

    public async ValueTask<Decision> AcquireAsync(LimitPolicy policy, string key)
    {
        if (TryEnterStore(out var isProbe))
        {
            try
            {
                var d = await AcquireRedisAsync(policy, key).ConfigureAwait(false);
                if (isProbe || Volatile.Read(ref _redisRetryAt) != 0) MarkStoreUp();
                return d;
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                MarkStoreDown(ex);
            }
            finally
            {
                if (isProbe) Volatile.Write(ref _probing, 0);
            }
        }
        return AcquireLocal(policy, key);
    }

    /// <summary>Healthy: everyone uses the store. Down: nobody does. Half-open: one probe at a time.</summary>
    private bool TryEnterStore(out bool isProbe)
    {
        isProbe = false;
        var retryAt = Volatile.Read(ref _redisRetryAt);
        if (retryAt == 0) return true;
        if (_time.GetTimestamp() < retryAt) return false;
        isProbe = Interlocked.CompareExchange(ref _probing, 1, 0) == 0;
        return isProbe;
    }

    private void MarkStoreDown(Exception ex)
    {
        var wasUp = Volatile.Read(ref _redisRetryAt) == 0;
        Volatile.Write(ref _redisRetryAt,
            _time.GetTimestamp() + (long)(_options.BreakerDuration.TotalSeconds * _time.TimestampFrequency));
        if (wasUp)
            _log.LogWarning(ex, "Shared limit store unavailable. Using local limits, probing every {Seconds}s", _options.BreakerDuration.TotalSeconds);
        else
            _log.LogDebug(ex, "Probe of shared limit store failed");
    }

    private void MarkStoreUp()
    {
        if (Interlocked.Exchange(ref _redisRetryAt, 0) != 0)
            _log.LogInformation("Shared limit store is back. Returning to shared limits");
    }

    /// <summary>Only needed for Concurrency policies. Pass the source returned by <see cref="AcquireAsync"/>.</summary>
    public async ValueTask ReleaseAsync(LimitPolicy policy, string key, DecisionSource source)
    {
        if (source == DecisionSource.Local)
        {
            if (_counters.TryGetValue(LocalKey(policy, key), out var c)) c.Release();
            return;
        }
        try
        {
            await WithDeadline(_redis.GetDatabase().ScriptEvaluateAsync(ConcurrencyReleaseLua, [RedisKeyFor(policy, key)])).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _log.LogWarning(ex, "Release failed. The in-flight counter expires on its own");
        }
    }

    // ---- shared store ----

    private async ValueTask<Decision> AcquireRedisAsync(LimitPolicy p, string key)
    {
        var db = _redis.GetDatabase();
        var k = RedisKeyFor(p, key);
        Task<RedisResult> call = p.Kind switch
        {
            LimitKind.TokenBucket => db.ScriptEvaluateAsync(TokenBucketLua, [k], [p.Rate, p.Burst, 1]),
            LimitKind.SlidingWindow => db.ScriptEvaluateAsync(SlidingWindowLua, [k],
                [(long)p.Window.TotalMilliseconds, p.Limit, Guid.NewGuid().ToString("N")]),
            LimitKind.Concurrency => db.ScriptEvaluateAsync(ConcurrencyAcquireLua, [k], [p.Limit, 30_000]),
            _ => throw new ArgumentOutOfRangeException(nameof(p))
        };
        var raw = await WithDeadline(call).ConfigureAwait(false);
        var r = (RedisResult[])raw!;
        return new Decision((long)r[0] == 1, TimeSpan.FromMilliseconds((long)r[1]), DecisionSource.Redis);
    }

    /// <summary>Throws TimeoutException if the store has not answered within the deadline. The abandoned call is observed so it cannot surface later.</summary>
    private async Task<T> WithDeadline<T>(Task<T> call)
    {
        try { return await call.WaitAsync(_options.StoreTimeout).ConfigureAwait(false); }
        catch (TimeoutException)
        {
            _ = call.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
            throw;
        }
    }

    private static RedisKey RedisKeyFor(LimitPolicy p, string key) => $"rl:{p.Name}:{key}";

    // ---- local fallback ----

    private string LocalKey(LimitPolicy p, string key) => $"{p.Name}:{key}";

    private Decision AcquireLocal(LimitPolicy p, string key)
    {
        var n = Math.Max(1, _options.Instances);
        var lk = LocalKey(p, key);
        switch (p.Kind)
        {
            case LimitKind.TokenBucket:
            {
                var rate = p.Rate / n;
                var burst = Math.Max(1, (int)Math.Ceiling(p.Burst / (double)n));
                var b = _buckets.GetOrAdd(lk, _ => new LocalBucket(burst, _time.GetTimestamp()));
                var (ok, retry) = b.TryTake(rate, burst, _time.GetTimestamp(), _time.TimestampFrequency);
                return new Decision(ok, retry, DecisionSource.Local);
            }
            case LimitKind.SlidingWindow:
            {
                var limit = Math.Max(1, (int)Math.Ceiling(p.Limit / (double)n));
                var w = _windows.GetOrAdd(lk, _ => new LocalWindow());
                var (ok, retry) = w.TryAdd(limit, p.Window, _time.GetTimestamp(), _time.TimestampFrequency);
                return new Decision(ok, retry, DecisionSource.Local);
            }
            default:
            {
                var limit = Math.Max(1, (int)Math.Ceiling(p.Limit / (double)n));
                var c = _counters.GetOrAdd(lk, _ => new LocalCounter());
                return new Decision(c.TryAcquire(limit), TimeSpan.Zero, DecisionSource.Local);
            }
        }
    }

    private sealed class LocalBucket(double tokens, long ts)
    {
        private double _tokens = tokens; private long _ts = ts; private readonly object _gate = new();
        public (bool, TimeSpan) TryTake(double rate, int burst, long now, long freq)
        {
            lock (_gate)
            {
                var elapsed = Math.Max(0, (now - _ts) / (double)freq);
                _tokens = Math.Min(burst, _tokens + elapsed * rate);
                _ts = now;
                if (_tokens >= 1) { _tokens -= 1; return (true, TimeSpan.Zero); }
                return (false, TimeSpan.FromSeconds((1 - _tokens) / rate));
            }
        }
    }

    private sealed class LocalWindow
    {
        private readonly Queue<long> _stamps = new(); private readonly object _gate = new();
        public (bool, TimeSpan) TryAdd(int limit, TimeSpan window, long now, long freq)
        {
            lock (_gate)
            {
                var span = (long)(window.TotalSeconds * freq);
                while (_stamps.Count > 0 && now - _stamps.Peek() >= span) _stamps.Dequeue();
                if (_stamps.Count < limit) { _stamps.Enqueue(now); return (true, TimeSpan.Zero); }
                return (false, TimeSpan.FromSeconds((span - (now - _stamps.Peek())) / (double)freq));
            }
        }
    }

    private sealed class LocalCounter
    {
        private int _n;
        public bool TryAcquire(int limit)
        {
            if (Interlocked.Increment(ref _n) <= limit) return true;
            Interlocked.Decrement(ref _n);
            return false;
        }
        public void Release() { if (Volatile.Read(ref _n) > 0) Interlocked.Decrement(ref _n); }
    }
}
