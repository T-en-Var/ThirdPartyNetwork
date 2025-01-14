using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public required NetworkUserDto NetworkUser { get; set; }
        public required RoleDto Role { get; set; }
    }
}
