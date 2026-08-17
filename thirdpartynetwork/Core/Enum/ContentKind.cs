namespace thirdpartynetwork.Core.Enum;

/// <summary>
/// PLAN.md §1: the discriminator used by every polymorphic content table
/// (ContentAccessGrant, ContentViewingLink, ContentModerationFlag) to say which
/// UserContentBaseModel-derived table ContentId points into. One value per content type —
/// NOT a [Flags] enum. Compare against Core.Enum.ContentKindFlags, which names a
/// *combination* of kinds for the Network.ExternalContentKinds policy toggle (§2.6).
/// </summary>
public enum ContentKind
{
    File = 0,
    Blog = 1,
    Configuration = 2,
    CustomPage = 3,
    PageBlock = 4,
}
