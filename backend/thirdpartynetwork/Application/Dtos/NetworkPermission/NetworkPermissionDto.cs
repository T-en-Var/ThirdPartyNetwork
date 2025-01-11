namespace thirdpartynetwork.Application.Dtos.NetworkPermission
{
    public class NetworkPermissionDto
    {
        public Guid NetworkId { get; set; }
        public Guid PermissionId { get; set; }
        public required bool IsRequired { get; set; }
        public ICollection<PermissionDto> Permissions { get; set; } = [];
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
