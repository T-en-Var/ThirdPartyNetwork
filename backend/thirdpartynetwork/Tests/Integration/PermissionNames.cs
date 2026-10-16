using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Tests.Integration;

public static class PermissionNames
{
    public static string Read(ContentKind kind) => kind switch
    {
        ContentKind.File => Constants.Permissions.ReadFile,
        ContentKind.Blog => Constants.Permissions.ReadBlog,
        ContentKind.Configuration => Constants.Permissions.ReadConfiguration,
        ContentKind.CustomPage => Constants.Permissions.ReadCustomPage,
        _ => throw new NotSupportedException(kind.ToString()),
    };

    public static string Manage(ContentKind kind) => kind switch
    {
        ContentKind.File => Constants.Permissions.ManageFile,
        ContentKind.Blog => Constants.Permissions.ManageBlog,
        ContentKind.Configuration => Constants.Permissions.ManageConfiguration,
        ContentKind.CustomPage => Constants.Permissions.ManageCustomPage,
        _ => throw new NotSupportedException(kind.ToString()),
    };

    public static string Moderate(ContentKind kind) => kind switch
    {
        ContentKind.File => Constants.Permissions.ModerateFile,
        ContentKind.Blog => Constants.Permissions.ModerateBlog,
        ContentKind.Configuration => Constants.Permissions.ModerateConfiguration,
        ContentKind.CustomPage => Constants.Permissions.ModerateCustomPage,
        _ => throw new NotSupportedException(kind.ToString()),
    };
}
