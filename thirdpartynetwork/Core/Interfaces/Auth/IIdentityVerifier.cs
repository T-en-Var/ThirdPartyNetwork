using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityVerifier
{
    Task<bool> IsContentOwnerAsync(Guid userProxyId, ContentKind contentKind, Guid targetContentId);
    Task<bool> IsUserOwnerAsync(Guid userProxyId, Guid targetUserId);
    Task<bool> IsUserProxyOwnerAsync(Guid userProxyId, Guid targetUserProxyId);
    Task<bool> IsNetworkUserOwnerAsync(Guid userProxyId, Guid targetNetworkUserId);
    Task<bool> IsAllowedAsync(Guid networkId, Guid userProxyId, string permissionName);
    Task<bool> HasPermissionAsync(Guid networkId, Guid userProxyId, string permissionName);
    Task<bool> IsNetworkAdminAsync(Guid networkId, Guid userProxyId);
    Task<bool> IsNetworkAdminOrHasPermissionAsync(Guid networkId, Guid userProxyId, string permissionName);
    Task<bool> HasSuperPermissionAsync(Guid userProxyId, string permissionName);
    Task<bool> IsSuperAdminAsync(Guid userProxyId);
    Task<bool> IsSuperAdminOrHasSuperPermissionAsync(Guid userProxyId, string permissionName);
}
