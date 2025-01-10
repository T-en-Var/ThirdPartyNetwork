namespace thirdpartynetwork.Core.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkPermission> NetworkPermissions { get; set; } = new List<NetworkPermission>();
        public ICollection<NetworkUserPermission> NetworkUserPermissions { get; set; } = new List<NetworkUserPermission>();
    }
}
