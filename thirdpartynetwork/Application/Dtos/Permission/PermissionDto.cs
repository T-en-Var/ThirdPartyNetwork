using thirdpartynetwork.Application.Dtos.NetworkAccess;
using thirdpartynetwork.Application.Dtos.NetworkUserAccess;

namespace thirdpartynetwork.Application.Dtos.Permission
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<RolePermissionDto> RolePermissions { get; set; } = [];
    }
}
