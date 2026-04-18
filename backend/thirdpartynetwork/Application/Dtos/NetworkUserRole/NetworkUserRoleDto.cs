using System.Text.Json.Serialization;
using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkUserRole
{
    public class NetworkUserRoleDto
    {
        public NetworkUserLightDto NetworkUser { get; set; }
        public required RoleLightDto Role { get; set; }
    }
    
    public class NetworkUserRoleNtRDto
    {
        public required RoleLightDto Role { get; set; }
    }
    
    public class NetworkUserRoleRtNDto
    {
        public required NetworkUserLightDto NetworkUser { get; set; }
    }
}
