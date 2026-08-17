using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

/// <summary>Return value of a successful ContentViewingLink resolution (§2.3).</summary>
public record ContentViewingLinkResolution(ContentViewingLink Link, UserContentBaseModel Content);

public interface IContentViewingLinkService
{
    Task<ContentViewingLink> MintAsync(
        ContentKind kind, Guid contentId,
        Guid? issuedForUserProxyId, Guid? issuedForNetworkId,
        ContentViewingLinkMode mode, TimeSpan? expiry);

    /// Looks up, validates (not found/expired/used-up), re-checks CURRENT access (defense in
    /// depth, §2.3), and consumes (marks UsedAt for OneTimeView) — returns null for any
    /// failure so the caller doesn't need to know which. `expectedKind`: pass ContentKind.File
    /// for GET /cdn/{token}; pass null for GET /view/{token} (accepts any kind EXCEPT File,
    /// which only ever resolves through /cdn).
    Task<ContentViewingLinkResolution?> ResolveAsync(Guid token, ContentKind? expectedKind);

    /// <summary>
    /// PLAN.md §6: the `ViewingUrl` every content DTO exposes, computed fresh per request.
    /// For File content that's fully public via an IsPublic network (§0.3), skips the table
    /// entirely and returns a stable public-bucket URL (§2.3's Mode=Permanent shortcut) —
    /// there's no equivalent object-storage benefit for the other four kinds, so those
    /// always mint a link. The minted link is addressed to the current caller
    /// (IssuedForUserProxyId) purely as access-delivery plumbing — per §2.1 this does NOT
    /// itself change the content's Tier(), only an unaddressed link does.
    /// </summary>
    Task<string?> ComputeViewingUrlAsync(UserContentBaseModel content, ContentKind kind, string requestScheme, string requestHost);
}
