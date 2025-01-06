namespace trojoaccounthub.Dtos.Role
{
    public class RoleDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkRoleDto> NetworkRoles { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
