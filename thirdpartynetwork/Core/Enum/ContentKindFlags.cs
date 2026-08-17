namespace thirdpartynetwork.Core.Enum;

/// <summary>
/// PLAN.md §2.6: backs Network.ExternalContentKinds — which of this network's Public
/// content kinds another network (never specifically invited via a ContentAccessGrant) may
/// durably reuse/embed. One bit per ContentKind member, same order. Deliberately a
/// SEPARATE enum from the sequential ContentKind discriminator: ContentKind names one kind
/// for a polymorphic FK, this one names a combination for a policy toggle.
/// </summary>
[Flags]
public enum ContentKindFlags
{
    None = 0,
    File = 1 << 0,
    Blog = 1 << 1,
    Configuration = 1 << 2,
    CustomPage = 1 << 3,
    PageBlock = 1 << 4,
}
