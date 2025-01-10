using thirdpartynetwork.Application.Dtos.NetworkRole;
using thirdpartynetwork.Application.Dtos.NetworkUserRole;

namespace thirdpartynetwork.Application.Dtos.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkRoleDto> NetworkRoles { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
