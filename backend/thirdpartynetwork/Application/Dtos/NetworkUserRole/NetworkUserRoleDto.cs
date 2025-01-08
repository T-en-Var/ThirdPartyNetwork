using thirdpartynetwork.Dtos.NetworkUser;
using thirdpartynetwork.Dtos.Role;

namespace thirdpartynetwork.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public ICollection<RoleDto> Roles { get; set; } = [];
    }
}
