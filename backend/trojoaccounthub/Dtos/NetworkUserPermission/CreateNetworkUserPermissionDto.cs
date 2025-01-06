namespace trojoaccounthub.Dtos.NetworkUserPermission
{
    public class CreateNetworkUserPermissionDto
    {
        public required bool IsAccepted { get; set; }
        public required int NetworkUserId { get; set; }
        public required int PermissionId { get; set; }
    }
}
