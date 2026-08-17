using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

/// <summary>
/// PLAN.md §2.5: implements the Q2 conclusion — moderators flag/hide, they don't edit.
/// ContentKind+ContentId is app-enforced (no DB FK), same polymorphic pattern as
/// ContentAccessGrant/ContentViewingLink.
/// </summary>
public class ContentModerationFlag : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    // Which network flagged it — a flag/hide is always scoped to one network's own
    // views/lists (§2.5), never global.
    public Guid NetworkId { get; set; }
    [JsonIgnore]
    public Network Network { get; set; }

    // Nullable + SetNull (DataContext), same "audit pointer outlives a transient
    // membership" reasoning as ContentAccessGrant.GrantedByNetworkUserId.
    public Guid? FlaggedByNetworkUserId { get; set; }
    [JsonIgnore]
    public NetworkUser? FlaggedByNetworkUser { get; set; }

    public string Reason { get; set; } = "";

    public ModerationFlagStatus Status { get; set; } = ModerationFlagStatus.Open;

    // Suppresses the content only within NetworkId's own views/lists — doesn't touch
    // PublishedAt or any ContentAccessGrant, so it never changes the content's Tier() for
    // anyone else (§2.5).
    public bool HiddenInNetwork { get; set; }
}
