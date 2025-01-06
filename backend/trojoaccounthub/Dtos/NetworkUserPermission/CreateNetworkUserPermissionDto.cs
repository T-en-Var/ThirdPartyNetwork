namespace trojoaccounthub.Dtos.NetworkUserPermission
{
    public class CreateNetworkUserPermissionDto
    {
        public required bool IsAccepted { get; set; }
        public required Guid NetworkUserId { get; set; }
        public required Guid PermissionId { get; set; }
    }
}
