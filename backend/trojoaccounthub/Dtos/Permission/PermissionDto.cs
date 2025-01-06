namespace trojoaccounthub.Dtos.Permission
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkPermissionDto> NetworkPermissions { get; set; } = [];
        public ICollection<NetworkUserPermissionDto> NetworkUserPermissions { get; set; } = [];
    }
}
