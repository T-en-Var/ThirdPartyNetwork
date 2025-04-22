using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class IdentityVerifier : IIdentityVerifier
{
    public bool IsContentOwner(Guid userProxyId, Guid targetContentId)
    {
        return true;
        // var content = contentService.GetById(targetContentId);
        // return content != null && content.CreatedBy == userProxyId;
    }

    public bool IsUserOwner(Guid userProxyId, Guid targetUserId)
    {
        return true;
        // var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        //
        // return userProxyQueryable.Any(u => u.User.Id == targetUserId);
    }

    public bool IsUserProxyOwner(Guid userProxyId, Guid targetUserProxyId)
    {
        return true;
        // if (userProxyId == targetUserProxyId) return true;
        //
        // var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        //
        // return userProxyQueryable.Any(u => 
        //     u.User.UserProxies.Any(n => n.Id == targetUserProxyId));
    }

    public bool IsNetworkUserOwner(Guid userProxyId, Guid targetNetworkUserId)
    {
        return true;
        // var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        //
        // return userProxyQueryable.Any(n => 
        //     n.NetworkUsers.Any(u => u.Id == targetNetworkUserId));
    }
    
    public bool IsAllowed(Guid networkId, Guid userProxyId, string permissionName)
        => CheckPermission(
            userProxyId, 
            permissionName, 
            includeNetworkId: networkId, 
            includeMainNetwork: true,
            checkForAdmin: true);

    public bool HasPermission(Guid networkId, Guid userProxyId, string permissionName)
        => CheckPermission(
            userProxyId, 
            permissionName, 
            includeNetworkId: networkId, 
            includeMainNetwork: false,
            checkForAdmin: false);

    public bool IsNetworkAdmin(Guid networkId, Guid userProxyId)
        => CheckPermission(
            userProxyId, 
            null, 
            includeNetworkId: 
            networkId, 
            includeMainNetwork: false,
            checkForAdmin: true);

    public bool IsNetworkAdminOrHasPermission(Guid networkId, Guid userProxyId, string permissionName)
        => CheckPermission(
            userProxyId, 
            permissionName, 
            includeNetworkId: networkId, 
            includeMainNetwork: false,
            checkForAdmin: true);

    public bool HasSuperPermission(Guid userProxyId, string permissionName)
        => CheckPermission(
            userProxyId, 
            permissionName, 
            includeNetworkId: null, 
            includeMainNetwork: true,
            checkForAdmin: false);

    public bool IsSuperAdmin(Guid userProxyId)
        => CheckPermission(
            userProxyId, 
            null, 
            includeNetworkId: null, 
            includeMainNetwork: true, 
            checkForAdmin: true);

    public bool IsSuperAdminOrHasSuperPermission(Guid userProxyId, string permissionName)
        => CheckPermission(
            userProxyId,
            permissionName, 
            includeNetworkId: null, 
            includeMainNetwork: true,
            checkForAdmin: true);

    private bool CheckPermission(
        Guid userProxyId, 
        string? permissionName, 
        Guid? includeNetworkId,
        bool includeMainNetwork, 
        bool checkForAdmin)
    {
        var queryable = Enumerable.Empty<Network>().AsQueryable();

        // if (includeNetworkId.HasValue)
        //     queryable = queryable.Concat(networkService.GetByIdQueryable(includeNetworkId.Value));
        //
        // if (includeMainNetwork)
        //     queryable = queryable.Concat(networkService.GetMainNetworkQueryable());

        return queryable.Any(network =>
            network.NetworkUsers.Any(user =>
                user.UserProxyId == userProxyId &&
                user.NetworkUserRoles.Any(role =>
                    (checkForAdmin 
                        && role.Role.Name == Constants.Roles.Admin) 
                    || (checkForAdmin 
                        && role.Role.RolePermissions.Any(p => 
                            p.Permission.Name == Constants.Permissions.Administrator)) 
                    || (!string.IsNullOrEmpty(permissionName) 
                        && role.Role.RolePermissions.Any(p => 
                            p.Permission.Name == permissionName))
                )
            )
        );
    }
}
