namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityVerifier
{
    bool IsContentOwner(Guid userProxyId, Guid targetContentId);
    bool IsUserOwner(Guid userProxyId, Guid targetUserId);
    /// <summary>
    /// Checks if the User(Proxy) owns the target User(Proxy); checks for sibling proxies.
    /// </summary>
    bool IsUserProxyOwner(Guid userProxyId, Guid targetUserProxyId);
    /// <summary>
    /// Does not check sibling proxies.
    /// </summary>
    bool IsNetworkUserOwner(Guid userProxyId, Guid targetNetworkUserId);
}
