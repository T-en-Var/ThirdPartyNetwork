namespace trojoaccounthub.Dtos.NetworkUserPermission
{
    public class NetworkUserPermissionDto
    {
        public required bool IsAccepted { get; set; }
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public ICollection<PermissionDto> Permissions { get; set; } = [];
    }
}
