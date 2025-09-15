namespace thirdpartynetwork.Core.Models
{
    public class NetworkUser : BaseModel
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }
        public Guid UserProxyId { get; set; }
        public required UserProxy UserProxy { get; set; }
        public required Network Network { get; set; }
        public bool AccessIncomplete { get; set; } = false;
        public ICollection<NetworkUserAccess> NetworkUserAccesses { get; set; } = new List<NetworkUserAccess>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    }
}
