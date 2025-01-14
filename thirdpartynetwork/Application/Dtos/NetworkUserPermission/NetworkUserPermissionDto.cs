using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Permission;

namespace thirdpartynetwork.Application.Dtos.NetworkUserPermission
{
    public class NetworkUserPermissionDto
    {
        public required bool IsAccepted { get; set; }
        public required NetworkUserDto NetworkUser { get; set; }
        public required PermissionDto Permission { get; set; }
    }
}
