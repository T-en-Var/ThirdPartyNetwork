namespace thirdpartynetwork.Core.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkRole> NetworkRoles { get; set; } = new List<NetworkRole>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
        public bool IsSystemProtected { get; set; } = false;
    }
}
