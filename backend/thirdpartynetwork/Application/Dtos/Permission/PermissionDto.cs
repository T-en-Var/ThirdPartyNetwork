using thirdpartynetwork.Application.Dtos.NetworkPermission;
using thirdpartynetwork.Application.Dtos.NetworkUserPermission;

namespace thirdpartynetwork.Application.Dtos.Permission
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
