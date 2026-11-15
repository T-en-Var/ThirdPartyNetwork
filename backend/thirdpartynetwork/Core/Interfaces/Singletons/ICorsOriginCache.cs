public interface ICorsOriginCache
{
    bool IsOriginAllowed(string origin);
    void AddOrigin(string newOrigin);
    void LoadOrigins(List<string> databaseOrigins);
}