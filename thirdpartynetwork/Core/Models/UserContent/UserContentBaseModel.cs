using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class UserContentBaseModel : BaseModel
{
    // Promoted up from each of the five subtypes (they each declared their own `Id` before)
    // — needed so ContentAccessGrant lookups (ContentKind+ContentId, §2.4) can be expressed
    // generically over T : UserContentBaseModel in AccessLevelVerifier, rather than per-type.
    public Guid Id { get; set; }

    // PLAN.md §2.1: retargeted from NetworkUser to UserProxy (nullable, SetNull on delete —
    // NOT the network-membership cascade this used to have). A durable persona, not a
    // membership row, is what makes AuthorOwned content actually portable across network
    // membership changes. Null (author's persona hard-deleted) falls back to NetworkOwned-
    // equivalent management — see IdentityVerifier.IsContentOwnerAsync.
    public Guid? AuthorUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? AuthorUserProxy { get; set; }

    // Tenancy scope — unrelated to ownership, always required. Backs the fast-path
    // PublicToNetwork-via-own-network cases, entitlement quotas, and moderation scoping.
    public Guid NetworkId { get; set; }
    [JsonIgnore]
    public Network Network { get; set; }

    // PLAN.md §2.1: promoted from Blog (its only previous owner) onto the base, so every
    // content type gets a real draft/publish lifecycle. Null = Draft. This is also the fact
    // Tier() reads first — see AccessLevelVerifier.
    public DateTime? PublishedAt { get; set; }

    // PLAN.md §2.1: AccessLevel is REMOVED as a stored column entirely (was here). Visibility
    // is now derived at read time from PublishedAt + ContentAccessGrant (+ ContentViewingLink)
    // — see AccessLevelVerifier.Tier(). Keeping a separately-set enum next to the grant table
    // would reintroduce the "two things that must independently agree" failure mode already
    // flagged for AccessLevelVerifier vs. GetAccessPredicateAsync (DATABASE_AND_AUTH_OVERVIEW.md §6.8).

    // Governs who may manage sharing/grants and who IsContentOwner resolves to — editability,
    // independent of the visibility derivation above.
    public OwnershipMode OwnershipMode { get; set; } = OwnershipMode.AuthorOwned;

    // Soft delete, generalized to all five content types (today only File hard-deletes).
    public DateTime? DeletedAt { get; set; }
}
