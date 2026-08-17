namespace thirdpartynetwork.Core.Enum;

/// <summary>
/// PLAN.md §2.4: what a ContentAccessGrant's grantee may do with the content. Comment/Like
/// are reserved, not enforced yet (§8, out of scope for this pass).
/// </summary>
[Flags]
public enum ContentCapabilities
{
    None = 0,
    View = 1 << 0,
    Edit = 1 << 1,
    Share = 1 << 2,
    Comment = 1 << 3,
    Like = 1 << 4,
}
