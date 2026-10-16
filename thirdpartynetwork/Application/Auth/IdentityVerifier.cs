using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class IdentityVerifier(DataContext context) : IIdentityVerifier
{
    public async Task<bool> IsContentOwnerAsync(Guid userProxyId, ContentKind contentKind, Guid targetContentId)
    {
        var content = await GetContentOwnershipFactsAsync(contentKind, targetContentId);
        return await IsOwnerOfAsync(userProxyId, content);
    }

    public async Task<bool> IsPageBlockOwnerAsync(Guid userProxyId, Guid pageBlockId)
    {
        var x = await context.PageBlocks.Where(f => f.Id == pageBlockId)
            .Select(f => new { f.AuthorUserProxyId, f.OwnershipMode, f.NetworkId })
            .FirstOrDefaultAsync();
        var content = x == null ? null : ((Guid?, OwnershipMode, Guid)?)(x.AuthorUserProxyId, x.OwnershipMode, x.NetworkId);

        return await IsOwnerOfAsync(userProxyId, content);
    }

    private async Task<bool> IsOwnerOfAsync(Guid userProxyId, (Guid? AuthorUserProxyId, OwnershipMode OwnershipMode, Guid NetworkId)? content)
    {
        if (content == null) return false;

        var (authorUserProxyId, ownershipMode, networkId) = content.Value;

        if (ownershipMode == OwnershipMode.AuthorOwned && authorUserProxyId.HasValue)
        {
            if (authorUserProxyId.Value == userProxyId) return true;

            return await context.UserProxies
                .Where(u => u.Id == userProxyId)
                .AnyAsync(u => u.User.UserProxies.Any(n => n.Id == authorUserProxyId.Value));
        }

        return await IsNetworkAdminAsync(networkId, userProxyId);
    }

    private async Task<(Guid? AuthorUserProxyId, OwnershipMode OwnershipMode, Guid NetworkId)?> GetContentOwnershipFactsAsync(
        ContentKind contentKind, Guid contentId)
    {
        switch (contentKind)
        {
            case ContentKind.File:
            {
                var x = await context.Files.Where(f => f.Id == contentId)
                    .Select(f => new { f.AuthorUserProxyId, f.OwnershipMode, f.NetworkId })
                    .FirstOrDefaultAsync();
                return x == null ? null : (x.AuthorUserProxyId, x.OwnershipMode, x.NetworkId);
            }
            case ContentKind.Blog:
            {
                var x = await context.Blogs.Where(f => f.Id == contentId)
                    .Select(f => new { f.AuthorUserProxyId, f.OwnershipMode, f.NetworkId })
                    .FirstOrDefaultAsync();
                return x == null ? null : (x.AuthorUserProxyId, x.OwnershipMode, x.NetworkId);
            }
            case ContentKind.Configuration:
            {
                var x = await context.Configurations.Where(f => f.Id == contentId)
                    .Select(f => new { f.AuthorUserProxyId, f.OwnershipMode, f.NetworkId })
                    .FirstOrDefaultAsync();
                return x == null ? null : (x.AuthorUserProxyId, x.OwnershipMode, x.NetworkId);
            }
            case ContentKind.CustomPage:
            {
                var x = await context.CustomPages.Where(f => f.Id == contentId)
                    .Select(f => new { f.AuthorUserProxyId, f.OwnershipMode, f.NetworkId })
                    .FirstOrDefaultAsync();
                return x == null ? null : (x.AuthorUserProxyId, x.OwnershipMode, x.NetworkId);
            }
            default:
                throw new NotSupportedException($"Unknown ContentKind {contentKind}");
        }
    }

    public async Task<bool> IsUserOwnerAsync(Guid userProxyId, Guid targetUserId)
    {
        return await context.UserProxies.AnyAsync(u => u.Id == userProxyId && u.User.Id == targetUserId);
    }

    public async Task<bool> IsUserProxyOwnerAsync(Guid userProxyId, Guid targetUserProxyId)
    {
        if (userProxyId == targetUserProxyId) return true;

        return await context.UserProxies
            .Where(u => u.Id == userProxyId)
            .AnyAsync(u => u.User.UserProxies.Any(n => n.Id == targetUserProxyId));
    }

    public async Task<bool> IsNetworkUserOwnerAsync(Guid userProxyId, Guid targetNetworkUserId)
    {
        return await context.UserProxies
            .Where(u => u.Id == userProxyId)
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