using thirdpartynetwork.Dtos.Permission;

namespace thirdpartynetwork.Dtos.NetworkPermission
{
    public class NetworkPermissionDto
    {
        public required bool IsRequired { get; set; }
        public ICollection<PermissionDto> Permissions { get; set; } = [];
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
