namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IJtiCacheService
{
    public class JtiTracker
    {
        public required string Jti { get; init; }
        public JtiTracker? Next { get; set; }
        public DateTime? Expiration { get; set; }
    }

    bool IsJtiValid(string jti);
    void StoreJti(string jti, string? previousJti, TimeSpan expiry);
    void RemoveJti(string jti);
}
