using System.Collections.Concurrent;

namespace thirdpartynetwork.Core.Helper.Singletons;

public class CorsOriginCache : ICorsOriginCache
{
    private ConcurrentDictionary<string, byte> _allowedOrigins = NewStore();

    public CorsOriginCache()
    {
        var defaultOrigin = Environment.GetEnvironmentVariable("FRONTEND_URL");

        if (!string.IsNullOrEmpty(defaultOrigin)) _allowedOrigins.TryAdd(Normalize(defaultOrigin), 0);
    }

    private static string Normalize(string origin) => origin.TrimEnd('/');

    private static ConcurrentDictionary<string, byte> NewStore() =>
        new(StringComparer.OrdinalIgnoreCase);

    public bool IsOriginAllowed(string origin)
    {
        return _allowedOrigins.ContainsKey(Normalize(origin));
    }

    public void AddOrigin(string newOrigin)
    {
        _allowedOrigins.TryAdd(Normalize(newOrigin), 0);
    }

    public void LoadOrigins(List<string> databaseOrigins)
    {
        var store = NewStore();

        foreach (var origin in databaseOrigins)
        {
            store.TryAdd(Normalize(origin), 0);
        }

        _allowedOrigins = store;
    }
}
