using thirdpartynetwork.Application.Dtos.FeatureFlag;

namespace thirdpartynetwork.Application.Dtos.Role
{
    public class UpdateRoleDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsDefault { get; set; }
        public required FeatureFlagDto FeatureFlags { get; set; }
    }
}
