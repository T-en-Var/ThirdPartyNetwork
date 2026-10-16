using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.Role
{
    public class CreateRoleDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsDefault { get; set; }
        public required JsonElement FeatureFlags { get; set; }
    }
}
