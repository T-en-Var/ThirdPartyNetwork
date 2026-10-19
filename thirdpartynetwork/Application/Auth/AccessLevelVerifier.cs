using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
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
        var caller = await LoadCallerAccessContextAsync(callerUserProxyId);

        var liveGrants = dataContext.ContentAccessGrants.Where(BuildLiveViewableGrantFilter(kind, model.Id, now));
        return await liveGrants.AnyAsync(BuildGranteeMatchPredicate(caller));
    }

    public async Task<Expression<Func<T, bool>>> GetAccessPredicateAsync<T>(Guid requestedNetworkId, string permissionName)
        where T : UserContentBaseModel
    {
        var kind = ContentKindMapper.From<T>();
        var now = DateTime.UtcNow;

        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);
        var caller = await LoadCallerAccessContextAsync(userProxy.Id);

        var hasPermission = caller.IsRealUser
            && await identityVerifier.IsAllowedAsync(requestedNetworkId, caller.CurrentUserId!.Value, permissionName);

        var granteeMatches = BuildGranteeMatchPredicate(caller);

        return model =>
            model.DeletedAt == null
            && (
                (caller.CurrentUserId.HasValue && model.AuthorUserProxyId == caller.CurrentUserId.Value)
                || (
                    !dataContext.ContentModerationFlags.Any(f =>
                        f.ContentKind == kind && f.ContentId == model.Id &&
                        f.NetworkId == requestedNetworkId && f.HiddenInNetwork)
                    && (
                        hasPermission
                        || (model.PublishedAt != null && dataContext.ContentAccessGrants
                                .Where(g => g.ContentKind == kind && g.ContentId == model.Id &&
                                            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
                                            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View)
                                .Any(granteeMatches))
                    )
                )
            );
    }

    public async Task<Expression<Func<T, bool>>> GetCrossNetworkAccessPredicateAsync<T>(string permissionName)
        where T : UserContentBaseModel
    {
        var kind = ContentKindMapper.From<T>();
        var now = DateTime.UtcNow;

        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);
        var caller = await LoadCallerAccessContextAsync(userProxy.Id);

        var hasUnlimitedAccess = caller.IsRealUser
            && await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(caller.CurrentUserId!.Value, permissionName);

        var permittedNetworkIds = new List<Guid>();
        if (caller.IsRealUser && !hasUnlimitedAccess)
        {
            permittedNetworkIds = await dataContext.NetworkUserRoles
                .Where(nur => nur.NetworkUser.UserProxyId == caller.CurrentUserId!.Value
                              && nur.Role.RolePermissions.Any(rp =>
                                  rp.Permission.Name == permissionName || rp.Permission.Name == Constants.Permissions.Administrator))
                .Select(nur => nur.NetworkUser.NetworkId)
                .Distinct()
                .ToListAsync();
        }

        var granteeMatches = BuildGranteeMatchPredicate(caller);

        return model =>
            model.DeletedAt == null
            && (
                (caller.CurrentUserId.HasValue && model.AuthorUserProxyId == caller.CurrentUserId.Value)
                || (
                    !dataContext.ContentModerationFlags.Any(f =>
                        f.ContentKind == kind && f.ContentId == model.Id &&
                        f.NetworkId == model.NetworkId && f.HiddenInNetwork)
                    && (
                        hasUnlimitedAccess
                        || permittedNetworkIds.Contains(model.NetworkId)
                        || (model.PublishedAt != null && dataContext.ContentAccessGrants
                                .Where(g => g.ContentKind == kind && g.ContentId == model.Id &&
                                            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
                                            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View)
                                .Any(granteeMatches))
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

    private sealed record CallerAccessContext(
        Guid? CurrentUserId,
        List<Guid> NetworkIds,
        List<Guid> RoleIds,
        List<Guid> PermissionIds)
    {
        public bool IsRealUser => CurrentUserId.HasValue;
    }

    private async Task<CallerAccessContext> LoadCallerAccessContextAsync(Guid callerUserProxyId)
    {
        if (callerUserProxyId == Guid.Empty)
            return new CallerAccessContext(null, [], [], []);

        var networkIds = await dataContext.NetworkUsers
            .Where(nu => nu.UserProxyId == callerUserProxyId)
            .Select(nu => nu.NetworkId)
            .ToListAsync();
        var roleIds = await dataContext.NetworkUserRoles
            .Where(nur => nur.NetworkUser.UserProxyId == callerUserProxyId)
            .Select(nur => nur.RoleId)
            .ToListAsync();
        var permissionIds = await dataContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return new CallerAccessContext(callerUserProxyId, networkIds, roleIds, permissionIds);
    }

    private static Expression<Func<ContentAccessGrant, bool>> BuildLiveViewableGrantFilter(
        ContentKind kind, Guid contentId, DateTime now)
    {
        return g =>
            g.ContentKind == kind && g.ContentId == contentId &&
            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View;
    }

    private static Expression<Func<ContentAccessGrant, bool>> BuildGranteeMatchPredicate(CallerAccessContext caller)
    {
        return g =>
            (caller.CurrentUserId.HasValue && g.GranteeType == GranteeType.User && g.GranteeUserProxyId == caller.CurrentUserId.Value)
            || (g.GranteeType == GranteeType.Network && g.GranteeNetworkId != null &&
                (caller.NetworkIds.Contains(g.GranteeNetworkId.Value) || g.GranteeNetwork!.IsPublic))
            || (caller.CurrentUserId.HasValue && g.GranteeType == GranteeType.NetworkRole && g.GranteeRoleId != null &&
                caller.RoleIds.Contains(g.GranteeRoleId.Value))
            || (caller.CurrentUserId.HasValue && g.GranteeType == GranteeType.NetworkPermission && g.GranteePermissionId != null &&
                caller.PermissionIds.Contains(g.GranteePermissionId.Value));
    }
}
