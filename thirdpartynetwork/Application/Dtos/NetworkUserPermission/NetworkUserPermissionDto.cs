using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Permission;

namespace thirdpartynetwork.Application.Dtos.NetworkUserPermission
{
    public class NetworkUserPermissionDto
    {
        public required bool IsAccepted { get; set; }
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public ICollection<PermissionDto> Permissions { get; set; } = [];
    }
}
