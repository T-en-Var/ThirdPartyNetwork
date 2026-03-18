using System.Text.Json;
using System.Text.Json.Serialization;
using thirdpartynetwork.Application.Dtos.NetworkUserRole;

namespace thirdpartynetwork.Application.Dtos.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsDefault { get; set; }
        public ICollection<RolePermissionRtPDto> RolePermissions { get; set; } = [];
        public ICollection<NetworkUserRoleRtNDto> NetworkUserRoles { get; set; } = [];
        public required JsonElement Entitlements { get; set; }
        public NetworkLightDto Network { get; set; }
    }
    
    public class RoleLightDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsDefault { get; set; }
        public ICollection<RolePermissionRtPDto> RolePermissions { get; set; } = [];
        public required JsonElement Entitlements { get; set; }
    }
}
