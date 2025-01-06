namespace trojoaccounthub.Dtos.NetworkUser
{
    public class NetworkUserDto
    {
        public int Id { get; set; }
        public required UserProxyDto UserProxy { get; set; }
        public ICollection<NetworkDto> Networks { get; set; } = [];
        public ICollection<NetworkUserPermissionDto> NetworkUserPermissions { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
