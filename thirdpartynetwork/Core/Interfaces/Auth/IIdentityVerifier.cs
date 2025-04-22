namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityVerifier
{
    bool IsContentOwner(Guid userProxyId, Guid targetContentId);
    bool IsUserOwner(Guid userProxyId, Guid targetUserId);
    bool IsUserProxyOwner(Guid userProxyId, Guid targetUserProxyId);
    bool IsNetworkUserOwner(Guid userProxyId, Guid targetNetworkUserId);
    bool IsAllowed(Guid networkId, Guid userProxyId, string permissionName);
    bool HasPermission(Guid networkId, Guid userProxyId, string permissionName);
    bool IsNetworkAdmin(Guid networkId, Guid userProxyId);
    bool IsNetworkAdminOrHasPermission(Guid networkId, Guid userProxyId, string permissionName);
    bool HasSuperPermission(Guid userProxyId, string permissionName);
    bool IsSuperAdmin(Guid userProxyId);
    bool IsSuperAdminOrHasSuperPermission(Guid userProxyId, string permissionName);
}
