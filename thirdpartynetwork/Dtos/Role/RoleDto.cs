using thirdpartynetwork.Dtos.NetworkRole;
using thirdpartynetwork.Dtos.NetworkUserRole;

namespace thirdpartynetwork.Dtos.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkRoleDto> NetworkRoles { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
