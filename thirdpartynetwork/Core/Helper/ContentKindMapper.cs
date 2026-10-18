using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Helper;

public static class ContentKindMapper
{
    public static ContentKind From<T>() where T : UserContentBaseModel => From(typeof(T));

    public static ContentKind From(Type type)
    {
        if (type == typeof(File)) return ContentKind.File;
        if (type == typeof(Blog)) return ContentKind.Blog;
        if (type == typeof(Configuration)) return ContentKind.Configuration;
        if (type == typeof(CustomPage)) return ContentKind.CustomPage;
        if (type == typeof(PageBlock)) return ContentKind.PageBlock;

        throw new NotSupportedException($"No ContentKind mapping for type {type.Name}");
    }

    public static string ReadPermissionFor(ContentKind kind) => kind switch
    {
        ContentKind.File => Constants.Permissions.ReadFile,
        ContentKind.Blog => Constants.Permissions.ReadBlog,
        ContentKind.Configuration => Constants.Permissions.ReadConfiguration,
        ContentKind.CustomPage => Constants.Permissions.ReadCustomPage,
        ContentKind.PageBlock => Constants.Permissions.ReadPageBlock,
        _ => throw new NotSupportedException($"No read permission mapping for {kind}")
    };
}
