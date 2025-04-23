using thirdpartynetwork.Application.Dtos.NetworkUserRole;

namespace thirdpartynetwork.Application.Dtos.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<RolePermissionDto> RolePermissions { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
        public NetworkDto Network { get; set; }
    }
}
