using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public ICollection<RoleDto> Roles { get; set; } = [];
    }
}
