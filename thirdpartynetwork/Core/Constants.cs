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
        
        public const string EditNetwork = "Manage Network";
        public const string EditNetworkRoles = "Manage Network Roles";
        public const string EditNetworkAccesses = "Manage Network Accesses";
        public const string AssignUserRoles = "Assign User Roles";
        public const string AssignAdminRoles = "Assign Admin Roles";
        
        public const string CreateNetwork = "Create a new Network";
        
        public const string UpdateSelf = "Update Self";
        public const string UpdateUser = "Update User";
        
        public const string KickUser = "Kick User";
        public const string KickUserAdmin = "Kick User Admin";
        public const string BanUser = "Ban User";
        public const string BanUserAdmin = "Ban User Admin";
        public const string TimoutUser = "Timout User";
        public const string TimoutUserAdmin = "Timout User Admin";
        
        public const string ViewContent = "View Content";
        public const string CreateContent = "Create Content";
        public const string EditContent = "Edit Content";
        public const string DeleteContent = "Delete Content";
        public const string ViewOwnContent = "View Own Content";
        public const string EditOwnContent = "Edit Own Content";
        public const string DeleteOwnContent = "Delete Own Content";
        
    }
}
