using thirdpartynetwork.Application.Dtos.NetworkUserPermission;
using thirdpartynetwork.Application.Dtos.NetworkUserRole;
using thirdpartynetwork.Application.Dtos.UserProxy;

namespace thirdpartynetwork.Application.Dtos.NetworkUser
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
