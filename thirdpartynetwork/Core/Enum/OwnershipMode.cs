namespace thirdpartynetwork.Core.Enum;

/// <summary>
/// PLAN.md §2.1/Q2: governs who manages sharing/grants and edits on a piece of content —
/// editability, independent of the visibility/Tier() derivation.
/// </summary>
public enum OwnershipMode
{
    AuthorOwned = 0,
    NetworkOwned = 1,
}
