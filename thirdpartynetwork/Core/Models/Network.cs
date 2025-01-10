namespace thirdpartynetwork.Core.Models
{
    public class Network
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Content> Contents { get; set; } = new List<Content>();
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<NetworkRole> NetworkRoles { get; set; } = new List<NetworkRole>();
        public ICollection<NetworkPermission> NetworkPermissions { get; set; } = new List<NetworkPermission>();
    }
}
