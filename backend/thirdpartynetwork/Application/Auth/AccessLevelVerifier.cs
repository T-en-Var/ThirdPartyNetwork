using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class AccessLevelVerifier(
    DataContext dataContext,
    IIdentityService identityService,
    IIdentityVerifier identityVerifier
) : IAccessLevelVerifier
{
    public async Task<bool> CheckFileAccessAsync(string fileId, Guid networkId, string permissionName)
    {
        var file = await dataContext.Files.FirstOrDefaultAsync(x => x.Id == ValueToGuid(fileId));
        return await CheckAccessAsync(file, networkId, permissionName);
    }

    public async Task<bool> CheckCustomPageAccessAsync(string customPageId, Guid networkId, string permissionName)
    {
        var customPage = await dataContext.CustomPages.FirstOrDefaultAsync(x => x.Id == ValueToGuid(customPageId));
        return await CheckAccessAsync(customPage, networkId, permissionName);
    }

    public async Task<bool> CheckPageBlockAccessAsync(string pageBlockId, Guid networkId, string permissionName)
    {
        // currently depends on parent custompage
        var pageBlock = await dataContext.PageBlocks.FirstOrDefaultAsync(x => x.Id == ValueToGuid(pageBlockId));
        return pageBlock != null && await CheckCustomPageAccessAsync(pageBlock.CustomPageId.ToString(), networkId, permissionName);
    }

    public async Task<bool> CheckConfigurationAccessAsync(string configurationId, Guid networkId, string permissionName)
    {
        var configuration = await dataContext.Configurations.FirstOrDefaultAsync(x => x.Id == ValueToGuid(configurationId));
        return await CheckAccessAsync(configuration, networkId, permissionName);
    }

    public async Task<bool> CheckBlogAccessAsync(string blogCompareValue, Guid networkId, string permissionName)
    {
        var blogId = ValueToGuid(blogCompareValue);
        var blog = blogId != Guid.Empty
            ? await dataContext.Blogs.FirstOrDefaultAsync(x => x.Id == blogId)
            : await dataContext.Blogs.FirstOrDefaultAsync(x => x.Slug == blogCompareValue);

        return await CheckAccessAsync(blog, networkId, permissionName);
    }

    public async Task<bool> CheckAccessAsync(UserContentBaseModel? model, Guid networkId, string permissionName)
    {
        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);
        return await CheckAccessForCallerAsync(model, networkId, userProxy.Id, permissionName);
    }

    public async Task<bool> CheckAccessForCallerAsync(UserContentBaseModel? model, Guid networkId, Guid callerUserProxyId, string permissionName)
    {
        if (model == null || model.DeletedAt != null) return false;

        var isRealUser = callerUserProxyId != Guid.Empty;
        if (isRealUser && model.AuthorUserProxyId == callerUserProxyId) return true;

        var kind = ContentKindMapper.From(model.GetType());

        if (await dataContext.ContentModerationFlags.AnyAsync(f =>
                f.ContentKind == kind && f.ContentId == model.Id && f.NetworkId == networkId && f.HiddenInNetwork))
            return false;

        if (isRealUser && await identityVerifier.IsAllowedAsync(networkId, callerUserProxyId, permissionName))
            return true;

        if (model.PublishedAt == null) return false;

        var now = DateTime.UtcNow;

        var grants = dataContext.ContentAccessGrants.Where(g =>
            g.ContentKind == kind && g.ContentId == model.Id &&
            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View);

        if (!isRealUser)
        {
            return await grants.AnyAsync(g =>
                g.GranteeType == GranteeType.Network && g.GranteeNetworkId != null && g.GranteeNetwork!.IsPublic);
        }

        var callerNetworkIds = await dataContext.NetworkUsers
            .Where(nu => nu.UserProxyId == callerUserProxyId)
            .Select(nu => nu.NetworkId)
            .ToListAsync();
        var callerRoleIds = await dataContext.NetworkUserRoles
            .Where(nur => nur.NetworkUser.UserProxyId == callerUserProxyId)
            .Select(nur => nur.RoleId)
            .ToListAsync();
        var callerPermissionIds = await dataContext.RolePermissions
            .Where(rp => callerRoleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return await grants.AnyAsync(g =>
            (g.GranteeType == GranteeType.User && g.GranteeUserProxyId == callerUserProxyId)
            || (g.GranteeType == GranteeType.Network && g.GranteeNetworkId != null &&
                (callerNetworkIds.Contains(g.GranteeNetworkId.Value) || g.GranteeNetwork!.IsPublic))
            || (g.GranteeType == GranteeType.NetworkRole && g.GranteeRoleId != null &&
                callerRoleIds.Contains(g.GranteeRoleId.Value))
            || (g.GranteeType == GranteeType.NetworkPermission && g.GranteePermissionId != null &&
                callerPermissionIds.Contains(g.GranteePermissionId.Value)));
    }

    public async Task<Expression<Func<T, bool>>> GetAccessPredicateAsync<T>(Guid requestedNetworkId, string permissionName)
        where T : UserContentBaseModel
    {
        var kind = ContentKindMapper.From<T>();
        var now = DateTime.UtcNow;

        Guid? currentUserId = null;
        var hasPermission = false;
        var callerNetworkIds = new List<Guid>();
        var callerRoleIds = new List<Guid>();
        var callerPermissionIds = new List<Guid>();

        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);
        var isRealUser = userProxy.Id != Guid.Empty;

        if (isRealUser)
        {
            currentUserId = userProxy.Id;

            hasPermission = await identityVerifier.IsAllowedAsync(requestedNetworkId, currentUserId.Value, permissionName);

            callerNetworkIds = await dataContext.NetworkUsers
                .Where(nu => nu.UserProxyId == currentUserId.Value)
                .Select(nu => nu.NetworkId)
                .ToListAsync();
            callerRoleIds = await dataContext.NetworkUserRoles
                .Where(nur => nur.NetworkUser.UserProxyId == currentUserId.Value)
                .Select(nur => nur.RoleId)
                .ToListAsync();
            callerPermissionIds = await dataContext.RolePermissions
                .Where(rp => callerRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

        return model =>
            model.DeletedAt == null
            && (
                (currentUserId.HasValue && model.AuthorUserProxyId == currentUserId.Value)
                || (
                    !dataContext.ContentModerationFlags.Any(f =>
                        f.ContentKind == kind && f.ContentId == model.Id &&
                        f.NetworkId == requestedNetworkId && f.HiddenInNetwork)
                    && (
                        hasPermission
                        || (model.PublishedAt != null && dataContext.ContentAccessGrants.Any(g =>
                                g.ContentKind == kind && g.ContentId == model.Id &&
                                g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
                                (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View &&
                                (
                                    (currentUserId.HasValue && g.GranteeType == GranteeType.User && g.GranteeUserProxyId == currentUserId.Value)
                                    || (g.GranteeType == GranteeType.Network && g.GranteeNetworkId != null &&
                                        (callerNetworkIds.Contains(g.GranteeNetworkId.Value) || g.GranteeNetwork!.IsPublic))
                                    || (currentUserId.HasValue && g.GranteeType == GranteeType.NetworkRole && g.GranteeRoleId != null &&
                                        callerRoleIds.Contains(g.GranteeRoleId.Value))
                                    || (currentUserId.HasValue && g.GranteeType == GranteeType.NetworkPermission && g.GranteePermissionId != null &&
                                        callerPermissionIds.Contains(g.GranteePermissionId.Value))
                                )))
                    )
                )
            );
    }

    public async Task<bool> CanNetworkExternallyAdoptAsync(UserContentBaseModel content, ContentKind kind, Guid consumingNetworkId)
    {
        if (content.NetworkId == consumingNetworkId) return true;
        if (content.DeletedAt != null) return false;

        var now = DateTime.UtcNow;

        var namedInvitation = await dataContext.ContentAccessGrants.AnyAsync(g =>
            g.ContentKind == kind && g.ContentId == content.Id &&
            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View &&
            g.GranteeType == GranteeType.Network && g.GranteeNetworkId == consumingNetworkId);
        if (namedInvitation) return true;

        if (content.PublishedAt == null) return false;

        var network = await dataContext.Networks.FirstOrDefaultAsync(n => n.Id == content.NetworkId);
        if (network == null) return false;

        var requiredFlag = kind switch
        {
            ContentKind.File => ContentKindFlags.File,
            ContentKind.Blog => ContentKindFlags.Blog,
            ContentKind.Configuration => ContentKindFlags.Configuration,
            ContentKind.CustomPage => ContentKindFlags.CustomPage,
            ContentKind.PageBlock => ContentKindFlags.PageBlock,
            _ => ContentKindFlags.None
        };
        if (!network.ExternalContentKinds.HasFlag(requiredFlag)) return false;

        return await dataContext.ContentAccessGrants.AnyAsync(g =>
            g.ContentKind == kind && g.ContentId == content.Id &&
            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now));
    }

    private static Guid ValueToGuid(string value)
    {
        return Guid.TryParse(value, out var result) ? result : Guid.Empty;
    }
}
