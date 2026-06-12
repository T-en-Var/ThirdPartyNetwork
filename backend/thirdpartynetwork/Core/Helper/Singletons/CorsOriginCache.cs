using System.Collections.Concurrent;

public class CorsOriginCache : ICorsOriginCache
{
    private ConcurrentDictionary<string, byte> _allowedOrigins = new();

    public CorsOriginCache()
    {
        var defaultOrigin = Environment.GetEnvironmentVariable("FRONTEND_URL");

        if (!string.IsNullOrEmpty(defaultOrigin)) _allowedOrigins.TryAdd(defaultOrigin, 0);
    }

    public bool IsOriginAllowed(string origin)
    {
        return _allowedOrigins.ContainsKey(origin);
    }

    public void AddOrigin(string newOrigin)
    {
        _allowedOrigins.TryAdd(newOrigin, 0);
    }

    public void LoadOrigins(List<string> databaseOrigins)
    {
        _allowedOrigins = new ConcurrentDictionary<string, byte>();

        foreach (var origin in databaseOrigins)
        {
            _allowedOrigins.TryAdd(origin, 0);
        }
    }
}