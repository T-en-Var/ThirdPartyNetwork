using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkRole
{
    public class NetworkRoleDto
    {
        public Guid NetworkId { get; set; }
        public Guid RoleId { get; set; }
        public required RoleDto Role { get; set; }
        public required NetworkDto Network { get; set; }
    }
}
