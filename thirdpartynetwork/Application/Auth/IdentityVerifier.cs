using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class IdentityVerifier(DataContext context) : IIdentityVerifier
{
    public bool IsContentOwner(Guid userProxyId, Guid targetContentId)
    {
        return true;
        // var content = contentService.GetById(targetContentId);
        // return content != null && content.CreatedBy == userProxyId;
    }

    public bool IsUserOwner(Guid userProxyId, Guid targetUserId)
    {
        var userProxyQueryable = context.UserProxies;
        
        return userProxyQueryable.Any(u => u.User.Id == targetUserId);
    }

    public bool IsUserProxyOwner(Guid userProxyId, Guid targetUserProxyId)
    {
        Console.WriteLine("Checking UserProxy: " + userProxyId + " -> target: " + targetUserProxyId);
        if (userProxyId == targetUserProxyId) return true;
        
        var userProxyQueryable = context.UserProxies;
        
        return userProxyQueryable.Any(u => 
            u.User.UserProxies.Any(n => n.Id == targetUserProxyId));
    }

    public bool IsNetworkUserOwner(Guid userProxyId, Guid targetNetworkUserId)
    {
        Console.WriteLine("Checking NetworkUser: " + userProxyId + " -> target: " + targetNetworkUserId);
        var userProxyQueryable = context.UserProxies.AsQueryable();
        userProxyQueryable = userProxyQueryable.Where(u => u.Id == userProxyId);
        
        return userProxyQueryable
            .Any(u => u.User.UserProxies
                .Any(u2 => u2.NetworkUsers
                    .Any(n => n.Id == targetNetworkUserId)));
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

    private IQueryable<Network> GetMainNetwork()
    {
        var mainNetwork = context.Networks
            .Where(n => n.Name == Constants.Networks.Main)
            .AsQueryable();

        if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");

        return mainNetwork;
    }

    private IQueryable<Network> GetNetworkById(Guid networkId)
    {
        var mainNetwork = context.Networks
            .Where(n => n.Id == networkId)
            .AsQueryable();

        return mainNetwork;
    }

    private bool CheckPermission(
        Guid userProxyId,
        string? permissionName,
        Guid? includeNetworkId,
        bool includeMainNetwork,
        bool checkForAdmin)
    {
        return GetMainNetwork().Any(network =>
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
           ) 
               || (includeNetworkId.HasValue && GetNetworkById(includeNetworkId.Value).Any(network =>
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
           ));
    }
}