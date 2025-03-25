using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class RoleVerifier(INetworkService networkService) : IRoleVerifier
{
    public bool HasPermission(Guid networkId, Guid userProxyId, string permissionName)
    {
        var queryableNetwork = networkService.GetByIdQueryable(networkId);
        
        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n => 
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r => 
                    r.Role.RolePermissions.Any(p => 
                        p.Permission.Name == permissionName))));
    }

    public bool IsNetworkAdmin(Guid networkId, Guid userProxyId)
    {
        var queryableNetwork = networkService.GetByIdQueryable(networkId);
        
        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n => 
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r => 
                    r.Role.RolePermissions.Any(p => 
                        p.Permission.Name == Constants.Permissions.Administrator))));
    }

    public bool IsNetworkAdminOrHasPermission(Guid networkId, Guid userProxyId, string permissionName)
    {
        // Combined functions IsNetworkAdmin + HasPermission in one call
        var queryableNetwork = networkService.GetByIdQueryable(networkId);
        
        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n => 
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r => 
                    r.Role.RolePermissions.Any(p => 
                        p.Permission.Name == Constants.Permissions.Administrator
                        || p.Permission.Name == permissionName))));
    }

    public bool HasSuperPermission(Guid userProxyId, string permissionName)
    {
        var queryableNetwork = networkService.GetMainNetworkQueryable();
        
        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n => 
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r => 
                    r.Role.RolePermissions.Any(p => 
                        p.Permission.Name == permissionName))));
    }

    public bool IsSuperAdmin(Guid userProxyId)
    {
        var queryableNetwork = networkService.GetMainNetworkQueryable();

        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n => 
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r => 
                    r.Role.Name == Constants.Roles.Admin 
                    || r.Role.RolePermissions.Any(p => 
                        p.Permission.Name == Constants.Permissions.Administrator))));
    }

    public bool IsSuperAdminOrHasSuperPermission(Guid userProxyId, string permissionName)
    {
        // Combined functions IsSuperAdmin + HasSuperPermission in one call
        var queryableNetwork = networkService.GetMainNetworkQueryable();

        return queryableNetwork.Any(qn =>
            qn.NetworkUsers.Any(n =>
                n.UserProxyId == userProxyId
                && n.NetworkUserRoles.Any(r =>
                    r.Role.Name == Constants.Roles.Admin
                    || r.Role.RolePermissions.Any(p =>
                        p.Permission.Name == Constants.Permissions.Administrator
                        || p.Permission.Name == permissionName))));
    }
}
