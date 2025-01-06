namespace trojoaccounthub.Dtos.NetworkRole
{
    public class NetworkRoleDto
    {
        public ICollection<RoleDto> Roles { get; set; } = [];
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
