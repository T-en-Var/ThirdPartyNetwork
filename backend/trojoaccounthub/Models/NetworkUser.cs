namespace trojoaccounthub.Models
{
    public class NetworkUser
    {
        public int Id { get; set; }
        public required int NetworkId { get; set; }
        public required int UserProxyId { get; set; }
        public required UserProxy UserProxy { get; set; }
        public required Network Network { get; set; }
        public ICollection<NetworkUserPermission> NetworkUserPermissions { get; set; } = new List<NetworkUserPermission>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    }
}
