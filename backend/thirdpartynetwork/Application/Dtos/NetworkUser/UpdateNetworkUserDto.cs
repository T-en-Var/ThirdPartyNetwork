using thirdpartynetwork.Application.Dtos.FeatureFlag;

namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class UpdateNetworkUserDto
    {
        public required FeatureFlagDto FeatureFlags { get; set; }
    }
}
