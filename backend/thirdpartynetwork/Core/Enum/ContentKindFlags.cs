namespace thirdpartynetwork.Core.Enum;

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
