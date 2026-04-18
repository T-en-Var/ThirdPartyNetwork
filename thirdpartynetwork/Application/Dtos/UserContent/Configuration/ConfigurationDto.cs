using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration
{
    public class ConfigurationDto : UserContentBaseDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
    }
    
    public class ConfigurationLightDto : UserContentBaseLightDto
    {
        public string Key { get; set; } = string.Empty;
        public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
    }
}