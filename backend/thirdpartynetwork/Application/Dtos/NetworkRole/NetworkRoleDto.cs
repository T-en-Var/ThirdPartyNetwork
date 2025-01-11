using thirdpartynetwork.Application.Dtos.Role;

namespace thirdpartynetwork.Application.Dtos.NetworkRole
{
    public class NetworkRoleDto
    {
        public Guid NetworkId { get; set; }
        public Guid RoleId { get; set; }
        public ICollection<RoleDto> Roles { get; set; } = [];
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
