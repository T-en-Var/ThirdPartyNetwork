using thirdpartynetwork.Application.Dtos.NetworkUserAccess;
using thirdpartynetwork.Application.Dtos.NetworkUserRole;
using thirdpartynetwork.Application.Dtos.UserProxy;

namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class NetworkUserDto
    {
        public Guid Id { get; set; }
        public required UserProxyDto UserProxy { get; set; }
        public required NetworkDto Network { get; set; }
        public ICollection<NetworkUserAccessDto> NetworkUserAccesses { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
