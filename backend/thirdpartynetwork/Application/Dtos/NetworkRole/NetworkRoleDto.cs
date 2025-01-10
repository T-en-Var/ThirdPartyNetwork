using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkRole
{
    public class NetworkRoleDto
    {
        public ICollection<RoleDto> Roles { get; set; } = [];
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
