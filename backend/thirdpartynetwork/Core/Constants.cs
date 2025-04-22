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
        public const string Main = "thirdpartynetwork";
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

        public const string ReadUser = "Read User";
        public const string ManageUser = "Manage User";
        public const string ManageOwnUser = "Manage Own User";

        public const string ReadContent = "Read Content";
        public const string ManageContent = "Manage Content";
        public const string ManageOwnContent = "Manage Own Content";

        // public const string KickUser = "Kick User";
        // public const string KickUserAdmin = "Kick User Admin";
        // public const string BanUser = "Ban User";
        // public const string BanUserAdmin = "Ban User Admin";
        // public const string TimoutUser = "Timout User";
        // public const string TimoutUserAdmin = "Timout User Admin";
    }

    public static string[] OwnerPermissions =
    [
        Permissions.ManageOwnContent
    ];
}