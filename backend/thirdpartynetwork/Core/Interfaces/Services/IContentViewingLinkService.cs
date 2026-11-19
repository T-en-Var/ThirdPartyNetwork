using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public record ContentViewingLinkResolution(ContentViewingLink Link, UserContentBaseModel Content);

public interface IContentViewingLinkService
{
    Task<ContentViewingLink> MintAsync(
        ContentKind kind, Guid contentId,
        Guid? issuedForUserProxyId, Guid? issuedForNetworkId,
        ContentViewingLinkMode mode, TimeSpan? expiry);

    Task<ContentViewingLinkResolution?> ResolveAsync(Guid token, ContentKind? expectedKind);

    Task<string?> ComputeViewingUrlAsync(UserContentBaseModel content, ContentKind kind, string requestScheme, string requestHost);
}
