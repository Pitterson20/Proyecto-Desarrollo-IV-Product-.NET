using System.Collections.Concurrent;

namespace Ferreteria.Auth;

/// <summary>
/// Réplica del RateLimiter de Laravel: cinco intentos fallidos por minuto y
/// por combinación correo|IP. Los inicios de sesión exitosos limpian el cupo.
/// </summary>
public sealed class LoginAttemptLimiter
{
    private const int MaxAttempts = 5;
    private const int WindowSeconds = 60;

    private readonly ConcurrentDictionary<string, List<long>> _attempts = new();

    public bool IsLimited(string key, out int retryAfterSeconds)
    {
        retryAfterSeconds = 0;

        if (!_attempts.TryGetValue(key, out var timestamps))
        {
            return false;
        }

        lock (timestamps)
        {
            Prune(timestamps);

            if (timestamps.Count < MaxAttempts)
            {
                return false;
            }

            var elapsed = Now - timestamps[0];
            retryAfterSeconds = Math.Max(
                1,
                (int)Math.Ceiling((WindowSeconds * 1000d - elapsed) / 1000d));
            return true;
        }
    }

    public void RegisterFailure(string key)
    {
        var timestamps = _attempts.GetOrAdd(key, _ => []);

        lock (timestamps)
        {
            Prune(timestamps);
            timestamps.Add(Now);
        }
    }

    public void Clear(string key) => _attempts.TryRemove(key, out _);

    private static long Now => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static void Prune(List<long> timestamps)
    {
        var threshold = Now - WindowSeconds * 1000;
        timestamps.RemoveAll(timestamp => timestamp <= threshold);
    }
}
