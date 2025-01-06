namespace trojoaccounthub.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public ICollection<RoleDto> Roles { get; set; } = [];
    }
}
