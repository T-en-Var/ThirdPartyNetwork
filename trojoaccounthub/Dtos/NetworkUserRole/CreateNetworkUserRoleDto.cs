namespace trojoaccounthub.Dtos.NetworkUserRole
{
    public class CreateNetworkUserRoleDto
    {
        public required Guid NetworkUserId { get; set; }
        public required Guid RoleId { get; set; }
    }
}
