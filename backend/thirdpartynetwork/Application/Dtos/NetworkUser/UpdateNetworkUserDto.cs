using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class UpdateNetworkUserDto
    {
        public required JsonElement FeatureFlags { get; set; }
    }
}
