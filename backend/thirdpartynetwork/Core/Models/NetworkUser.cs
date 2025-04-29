namespace thirdpartynetwork.Core.Models
{
    public class NetworkUser
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public required UserProxy UserProxy { get; set; }
        public required Network Network { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public ICollection<NetworkUserAccess> NetworkUserAccesses { get; set; } = new List<NetworkUserAccess>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    }
}
