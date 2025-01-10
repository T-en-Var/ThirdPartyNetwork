namespace thirdpartynetwork.Core.Models
{
    public class NetworkUser
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public required UserProxy UserProxy { get; set; }
        public required Network Network { get; set; }
        public ICollection<NetworkUserPermission> NetworkUserPermissions { get; set; } = new List<NetworkUserPermission>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    }
}
