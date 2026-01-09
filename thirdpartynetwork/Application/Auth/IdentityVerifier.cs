using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class IdentityVerifier(DataContext context) : IIdentityVerifier
{
    public Task<bool> IsContentOwnerAsync(Guid userProxyId, Guid targetContentId)
    {
        throw new NotImplementedException();
        // var content = contentService.GetById(targetContentId);
        // return content != null && content.CreatedBy == userProxyId;
    }

    public async Task<bool> IsUserOwnerAsync(Guid userProxyId, Guid targetUserId)
    {
        var userProxyQueryable = context.UserProxies;

        return await userProxyQueryable.AnyAsync(u => u.User.Id == targetUserId);
    }

    public async Task<bool> IsUserProxyOwnerAsync(Guid userProxyId, Guid targetUserProxyId)
    {
        if (userProxyId == targetUserProxyId) return true;

        var userProxyQueryable = context.UserProxies;

        return await userProxyQueryable.AnyAsync(u =>
            u.User.UserProxies.Any(n => n.Id == targetUserProxyId)); // Inner Any is translated to EXISTS
    }

    public async Task<bool> IsNetworkUserOwnerAsync(Guid userProxyId, Guid targetNetworkUserId)
    {
        if (userProxyId == targetNetworkUserId) return true;

        var userProxyQueryable = context.UserProxies.AsQueryable();
        userProxyQueryable = userProxyQueryable.Where(u => u.Id == userProxyId);

        return await userProxyQueryable
            .AnyAsync(u => u.User.UserProxies
                .Any(u2 => u2.NetworkUsers
                    .Any(n => n.Id == targetNetworkUserId)));
    }

    public async Task<bool> IsAllowedAsync(Guid networkId, Guid userProxyId, string permissionName)
        => await CheckPermissionAsync(
            userProxyId,
            permissionName,
            includeNetworkId: networkId,
            includeMainNetwork: true,
            checkForAdmin: true);

    public async Task<bool> HasPermissionAsync(Guid networkId, Guid userProxyId, string permissionName)
        => await CheckPermissionAsync(
            userProxyId,
            permissionName,
            includeNetworkId: networkId,
            includeMainNetwork: false,
            checkForAdmin: false);

    public async Task<bool> IsNetworkAdminAsync(Guid networkId, Guid userProxyId)
        => await CheckPermissionAsync(
            userProxyId,
            null,
            includeNetworkId:
            networkId,
            includeMainNetwork: false,
            checkForAdmin: true);

    public async Task<bool> IsNetworkAdminOrHasPermissionAsync(Guid networkId, Guid userProxyId, string permissionName)
        => await CheckPermissionAsync(
            userProxyId,
            permissionName,
            includeNetworkId: networkId,
            includeMainNetwork: false,
            checkForAdmin: true);

    public async Task<bool> HasSuperPermissionAsync(Guid userProxyId, string permissionName)
        => await CheckPermissionAsync(
            userProxyId,
            permissionName,
            includeNetworkId: null,
            includeMainNetwork: true,
            checkForAdmin: false);

    public async Task<bool> IsSuperAdminAsync(Guid userProxyId)
        => await CheckPermissionAsync(
            userProxyId,
            null,
            includeNetworkId: null,
            includeMainNetwork: true,
            checkForAdmin: true);

    public async Task<bool> IsSuperAdminOrHasSuperPermissionAsync(Guid userProxyId, string permissionName)
        => await CheckPermissionAsync(
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

    private async Task<bool> CheckPermissionAsync(
        Guid userProxyId,
        string? permissionName,
        Guid? includeNetworkId,
        bool includeMainNetwork,
        bool checkForAdmin)
    {
        if (includeMainNetwork && await CheckQueryableAsync(GetMainNetwork(), permissionName, userProxyId, checkForAdmin))
            return true;

        if (includeNetworkId.HasValue && await CheckQueryableAsync(GetNetworkById(includeNetworkId.Value), permissionName, userProxyId, checkForAdmin))
            return true;

        return false;
    }

    private static async Task<bool> CheckQueryableAsync(
        IQueryable<Network> networkQueryable,
        string? permissionName,
        Guid userProxyId,
        bool checkForAdmin)
    {
        var checkForPerm = !string.IsNullOrEmpty(permissionName);
        var result = await networkQueryable.AsQueryable().AnyAsync(network =>
            network.NetworkUsers.Any(nu =>
                nu.UserProxyId == userProxyId &&
                nu.NetworkUserRoles.Any(nur =>
                    (checkForAdmin && nur.Role.RolePermissions.Any(p =>
                            p.Permission.Name == Constants.Permissions.Administrator))
                    || (checkForPerm
                        && nur.Role.RolePermissions.Any(rp =>
                            rp.Permission.Name == permissionName))
                )
            )
        );
        return result;
    }
}