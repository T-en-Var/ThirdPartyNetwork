using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

/// <summary>
/// PLAN.md §2.4: the generic sharing primitive for all five UserContentBaseModel-derived
/// types. ContentKind+ContentId is app-enforced (no DB FK) — same un-constrained-FK pattern
/// already used for PageBlock.ParentPageId — so one table serves all five content types
/// instead of five near-identical join tables (§1).
/// </summary>
public class ContentAccessGrant : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    public GranteeType GranteeType { get; set; }

    public Guid? GranteeNetworkId { get; set; }
    [JsonIgnore]
    public Network? GranteeNetwork { get; set; }

    public Guid? GranteeRoleId { get; set; }
    [JsonIgnore]
    public Role? GranteeRole { get; set; }

    public Guid? GranteePermissionId { get; set; }
    [JsonIgnore]
    public Permission? GranteePermission { get; set; }

    public Guid? GranteeUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? GranteeUserProxy { get; set; }

    public ContentCapabilities Capabilities { get; set; } = ContentCapabilities.View;

    // Audit: who performed this administrative action, in which network capacity — stays
    // NetworkUser (not UserProxy) since granting requires holding ManageContentAccess/
    // IsContentOwner within a specific network, an inherently network-scoped action.
    // Nullable + SetNull (DataContext) so the grant outlives the granter's own membership —
    // same "don't tie a real thing to a transient membership" reasoning as
    // UserContentBaseModel.AuthorUserProxyId.
    public Guid? GrantedByNetworkUserId { get; set; }
    [JsonIgnore]
    public NetworkUser? GrantedByNetworkUser { get; set; }

    // Self-expiring share ("share for the next 7 days") — needs no explicit revoke later.
    public DateTime? ExpiresAt { get; set; }

    // Manual, early soft revoke — audit trail of who had access and when it was pulled,
    // rather than deleting the row (Q2: moderating shouldn't make history vanish).
    public DateTime? RevokedAt { get; set; }
}
