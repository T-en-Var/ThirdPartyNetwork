namespace thirdpartynetwork.Core.Models
{
    public class Access
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        public ICollection<NetworkUserAccess> NetworkUserAccesses { get; set; } = new List<NetworkUserAccess>();
        public bool IsSystemProtected { get; set; } = false;
    }
}
