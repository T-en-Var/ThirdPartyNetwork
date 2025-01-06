using thirdpartynetwork.Dtos.NetworkUserPermission;
using thirdpartynetwork.Dtos.NetworkUserRole;
using thirdpartynetwork.Dtos.UserProxy;

namespace thirdpartynetwork.Dtos.NetworkUser
{
    public class NetworkUserDto
    {
        public Guid Id { get; set; }
        public required UserProxyDto UserProxy { get; set; }
        public ICollection<NetworkDto> Networks { get; set; } = [];
        public ICollection<NetworkUserPermissionDto> NetworkUserPermissions { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
