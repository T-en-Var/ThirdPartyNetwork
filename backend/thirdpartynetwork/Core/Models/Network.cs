namespace thirdpartynetwork.Core.Models
{
    public class Network
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Content> Contents { get; set; } = new List<Content>();
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        public bool IsSystemProtected { get; set; } = false;
    }
}
