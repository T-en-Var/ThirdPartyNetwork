using thirdpartynetwork.Dtos.NetworkPermission;
using thirdpartynetwork.Dtos.NetworkUserPermission;

namespace thirdpartynetwork.Dtos.Permission
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkPermissionDto> NetworkPermissions { get; set; } = [];
        public ICollection<NetworkUserPermissionDto> NetworkUserPermissions { get; set; } = [];
    }
}
