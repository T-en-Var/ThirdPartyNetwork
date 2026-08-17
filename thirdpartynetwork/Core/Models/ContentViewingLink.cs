using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

/// <summary>
/// PLAN.md §2.3: the opaque, unauthenticated share-link broker, generalized across all five
/// content kinds (ContentKind+ContentId, polymorphic — same pattern as ContentAccessGrant).
/// `Id` IS the token used in the public URL (GET /cdn/{token} for File, GET /view/{token}
/// for the other four kinds).
/// </summary>
public class ContentViewingLink : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    // Null = unaddressed/shareable link ("anyone holding this URL") — one of the two things
    // that makes content Unlisted under Tier() (§2.1); non-null = minted for a specific,
    // already-authorized caller purely as access-delivery plumbing, and doesn't itself
    // change the tier.
    public Guid? IssuedForUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? IssuedForUserProxy { get; set; }

    public Guid? IssuedForNetworkId { get; set; }
    [JsonIgnore]
    public Network? IssuedForNetwork { get; set; }

    public ContentViewingLinkMode Mode { get; set; } = ContentViewingLinkMode.OneTimeView;

    public DateTime? ExpiresAt { get; set; }

    // Set on first render for Mode=OneTimeView — a used one-time link is dead regardless of
    // its nominal ExpiresAt (§2.1/§2.3).
    public DateTime? UsedAt { get; set; }
}
