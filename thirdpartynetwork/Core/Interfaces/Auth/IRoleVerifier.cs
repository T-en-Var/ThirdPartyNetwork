namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IRoleVerifier
{
    bool HasPermission(Guid networkId, Guid userProxyId, string permissionName);
    bool IsNetworkAdmin(Guid networkId, Guid userProxyId);
    bool IsNetworkAdminOrHasPermission(Guid networkId, Guid userProxyId, string permissionName);
    bool HasSuperPermission(Guid userProxyId, string permissionName);
    bool IsSuperAdmin(Guid userProxyId);
    bool IsSuperAdminOrHasSuperPermission(Guid userProxyId, string permissionName);
}
