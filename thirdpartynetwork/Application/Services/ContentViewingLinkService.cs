using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Interfaces.Services;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Services;

/// <summary>PLAN.md §2.3.</summary>
public class ContentViewingLinkService(
    IContentViewingLinkRepository repository,
    IFileRepository fileRepository,
    IBlogRepository blogRepository,
    IConfigurationRepository configurationRepository,
    ICustomPageRepository customPageRepository,
    IPageBlockRepository pageBlockRepository,
    IAccessLevelVerifier accessLevelVerifier,
    IIdentityService identityService,
    ICDNService cdnService,
    DataContext dataContext) : IContentViewingLinkService
{
    public async Task<ContentViewingLink> MintAsync(
        ContentKind kind, Guid contentId,
        Guid? issuedForUserProxyId, Guid? issuedForNetworkId,
        ContentViewingLinkMode mode, TimeSpan? expiry)
    {
        var link = new ContentViewingLink
        {
            Id = Guid.NewGuid(),
            ContentKind = kind,
            ContentId = contentId,
            IssuedForUserProxyId = issuedForUserProxyId,
            IssuedForNetworkId = issuedForNetworkId,
            Mode = mode,
            ExpiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null,
        };

        return await repository.CreateAsync(link);
    }

    public async Task<ContentViewingLinkResolution?> ResolveAsync(Guid token, ContentKind? expectedKind)
    {
        var link = await repository.GetByIdAsync(token);
        if (link == null) return null;

        if (expectedKind.HasValue && link.ContentKind != expectedKind.Value) return null;
        // File only ever resolves through GET /cdn/{token} (bytes redirect) — GET
        // /view/{token} (expectedKind == null) is for the other four kinds only (§2.3).
        if (!expectedKind.HasValue && link.ContentKind == ContentKind.File) return null;

        if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= DateTime.UtcNow) return null;
        if (link.Mode == ContentViewingLinkMode.OneTimeView && link.UsedAt != null) return null;

        var content = await GetContentAsync(link.ContentKind, link.ContentId);
        if (content == null) return null;

        // PageBlock's own visibility is vestigial — delegate the actual recheck to its
        // parent CustomPage, same as AccessLevelVerifier.CheckPageBlockAccessAsync already
        // does for the authenticated per-item path (DATABASE_AND_AUTH_OVERVIEW.md §5.3).
        UserContentBaseModel? accessCheckContent = content;
        var accessCheckKind = link.ContentKind;
        if (link.ContentKind == ContentKind.PageBlock)
        {
            accessCheckContent = await customPageRepository.GetByIdBypassingAccessAsync(((PageBlock)content).CustomPageId);
            accessCheckKind = ContentKind.CustomPage;
        }

        if (accessCheckContent == null) return null;

        // Defense in depth (§2.3): access may have been revoked after the link was minted
        // but before it was used — re-check against the caller it was minted for, not
        // whoever is making this particular (deliberately unauthenticated) HTTP request.
        var callerId = link.IssuedForUserProxyId ?? Guid.Empty;
        var readPermission = ContentKindMapper.ReadPermissionFor(accessCheckKind);
        var stillAllowed = await accessLevelVerifier.CheckAccessForCallerAsync(
            accessCheckContent, accessCheckContent.NetworkId, callerId, readPermission);
        if (!stillAllowed) return null;

        if (link.Mode == ContentViewingLinkMode.OneTimeView)
        {
            await repository.MarkUsedAsync(link);
        }

        return new ContentViewingLinkResolution(link, content);
    }

    public async Task<string?> ComputeViewingUrlAsync(UserContentBaseModel content, ContentKind kind, string requestScheme, string requestHost)
    {
        if (content.DeletedAt != null) return null;

        if (kind == ContentKind.File && await IsFullyPublicAsync(content, kind))
        {
            var file = (File)content;
            return cdnService.GetPublicUrl(file.Bucket, file.ObjectKey).ToString();
        }

        var caller = await identityService.GetUserProxyAsync(omitThrow: true);
        var isStreaming = kind == ContentKind.File && ((File)content).Duration.HasValue;

        var link = await MintAsync(
            kind, content.Id,
            issuedForUserProxyId: caller.Id == Guid.Empty ? null : caller.Id,
            issuedForNetworkId: null,
            mode: isStreaming ? ContentViewingLinkMode.TimeLimited : ContentViewingLinkMode.OneTimeView,
            expiry: isStreaming ? cdnService.StreamingPresignExpiry : cdnService.DefaultPresignExpiry);

        var path = kind == ContentKind.File ? "cdn" : "view";
        return $"{requestScheme}://{requestHost}/{path}/{link.Id}";
    }

    // Mirrors AccessLevelVerifier's §0.3 anonymous-reachability check — used here only to
    // decide the Mode=Permanent/no-row shortcut, not as an authorization decision itself.
    private async Task<bool> IsFullyPublicAsync(UserContentBaseModel content, ContentKind kind)
    {
        if (content.PublishedAt == null) return false;

        var now = DateTime.UtcNow;
        return await dataContext.ContentAccessGrants.AnyAsync(g =>
            g.ContentKind == kind && g.ContentId == content.Id &&
            g.RevokedAt == null && (g.ExpiresAt == null || g.ExpiresAt > now) &&
            (g.Capabilities & ContentCapabilities.View) == ContentCapabilities.View &&
            g.GranteeType == GranteeType.Network && g.GranteeNetworkId != null && g.GranteeNetwork!.IsPublic);
    }

    private async Task<UserContentBaseModel?> GetContentAsync(ContentKind kind, Guid id) => kind switch
    {
        ContentKind.File => await fileRepository.GetByIdBypassingAccessAsync(id),
        ContentKind.Blog => await blogRepository.GetByIdBypassingAccessAsync(id),
        ContentKind.Configuration => await configurationRepository.GetByIdBypassingAccessAsync(id),
        ContentKind.CustomPage => await customPageRepository.GetByIdBypassingAccessAsync(id),
        ContentKind.PageBlock => await pageBlockRepository.GetByIdBypassingAccessAsync(id),
        _ => throw new NotSupportedException($"Unknown ContentKind {kind}")
    };
}
