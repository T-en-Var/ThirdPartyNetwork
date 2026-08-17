namespace thirdpartynetwork.Core;

public static class Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Client = "Client";
        public const string User = "User";
    }

    public static class Networks
    {
        public const string Main = "TrojoNetworks";
    }

    public static class Access
    {
        public const string AccessUsername = "Access Username";
        public const string AccessFirstname = "Access Firstname";
        public const string AccessLastname = "Access Lastname";
        public const string AccessEmail = "Access Email";
        public const string AccessProfilePicture = "Access ProfilePicture";
    }

    public static class Permissions
    {
        public const string SystemManaged = "System Managed";

        public const string Administrator = "Administrator";
        // public const string OnlyOwner = "Only Recourse Owner";

        public const string ReadNetwork = "Read Network";
        public const string ManageNetwork = "Manage Network";

        public const string ReadAccess = "Read Access";
        public const string ManageAccess = "Manage Access";

        public const string ReadPermission = "Read Permission";
        public const string ManagePermission = "Manage Permission";

        public const string ReadRole = "Read Role";
        public const string ManageRole = "Manage Role";

        public const string IsUserOwner = "IsUserOwner";
        public const string IsNetworkUserOwner = "IsNetworkUserOwner";
        public const string IsUserProxyOwner = "IsUserProxyOwner";
        public const string IsContentOwner = "IsContentOwner";

        // PLAN.md §3.4: grant/revoke sharing (OR'd with IsContentOwner on the access-grant
        // endpoints, §2.4/§6) — deliberately NOT one of the existing Manage<Type>
        // permissions, which stay edit/delete-only per Q2.
        public const string ManageContentAccess = "Manage Content Access";

        // PLAN.md §2.5/§3.4: flag/hide only, never mutate (Q2) — wired up in full in the
        // ContentModerationFlag pass; declared here now since seeding/permission constants
        // are naturally one place, not split across passes.
        public const string ModerateFile = "Moderate File";
        public const string ModerateBlog = "Moderate Blog";
        public const string ModerateConfiguration = "Moderate Configuration";
        public const string ModerateCustomPage = "Moderate CustomPage";
        public const string ModeratePageBlock = "Moderate PageBlock";


        public const string ReadUser = "Read User";
        public const string ManageUser = "Manage User";

        public const string ReadCustomPage = "Read CustomPage";
        public const string ManageCustomPage = "Manage CustomPage";

        public const string ReadPageBlock = "Read PageBlock";
        public const string ManagePageBlock = "Manage PageBlock";

        public const string ReadFile = "Read File";
        public const string ManageFile = "Manage File";
        
        public const string ReadConfiguration = "Read Configuration";
        public const string ManageConfiguration = "Manage Configuration";
        public const string ReadBlog = "Read Blog";
        public const string ManageBlog = "Manage Blog";

        // public const string KickUser = "Kick User";
        // public const string KickUserAdmin = "Kick User Admin";
        // public const string BanUser = "Ban User";
        // public const string BanUserAdmin = "Ban User Admin";
        // public const string TimoutUser = "Timout User";
        // public const string TimoutUserAdmin = "Timout User Admin";
    }

    public static readonly string[] GeneralPermissions = [
        Permissions.ReadAccess,
        Permissions.ReadPermission,
    ];

    public static readonly string[] SlugContentPermissions = [
        Permissions.ReadBlog,
    ];

    public static readonly string[] ContentPermissions = [
        Permissions.ReadCustomPage,
        // Permissions.ManageCustomPage,
        Permissions.ReadPageBlock,
        // Permissions.ManagePageBlock,
        Permissions.ReadFile,
        // Permissions.ManageFile,
        Permissions.ReadConfiguration,
            Permissions.ReadBlog,
        // Permissions.ManageConfiguration,
    ];
    
    public static readonly string[] OwnerPermissions =
    [
        Permissions.IsUserOwner,
        Permissions.IsNetworkUserOwner,
        Permissions.IsUserProxyOwner,
        Permissions.IsContentOwner,
        // Permissions.ManageUser,
        // Permissions.ReadUser,
    ];

    // PLAN.md §4: narrows Manage<Type> from "any role-holder can edit/delete any member's
    // content" to author-only (AuthorOwned) / author-or-admin (NetworkOwned) — i.e. exactly
    // IsContentOwnerAsync, resolved via route content-id the same way ContentPermissions
    // resolves theirs. Only applies when the route actually names a content id (Update/
    // Delete); a bare Create action has nothing yet to own, so it falls through to the
    // ordinary role/permission check untouched. **Explicitly flagged behavior change** — see
    // PLAN.md §4 for the callers-relying-on-the-old-behavior caveat.
    public static readonly string[] ContentManagePermissions =
    [
        Permissions.ManageFile,
        Permissions.ManageBlog,
        Permissions.ManageConfiguration,
        Permissions.ManageCustomPage,
        Permissions.ManagePageBlock,
    ];
}