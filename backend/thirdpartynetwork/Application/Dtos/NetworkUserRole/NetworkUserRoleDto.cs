using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public NetworkUserDto NetworkUser { get; set; }
        public RoleDto Role { get; set; }
    }
}
