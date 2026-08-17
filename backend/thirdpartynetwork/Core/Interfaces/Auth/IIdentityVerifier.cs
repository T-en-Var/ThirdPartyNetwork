using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityVerifier
{
    // PLAN.md §3.2: takes ContentKind explicitly rather than searching all five content
    // tables for a matching id — this had no callers before (the stub only ever threw), so
    // this is a clean signature, not a breaking change to real behavior.
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
