using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration
{
    public class ConfigurationDto : CustomUserContentBaseDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
    }
    
    public class ConfigurationLightDto : CustomUserContentBaseLightDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
    }
}