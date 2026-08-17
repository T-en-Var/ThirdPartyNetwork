using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration;

public class CreateConfigurationDto : CreateUserContentBaseDto
{
    public required string Key { get; set; }
    public required JsonDocument Value { get; set; }
    // AccessLevel removed (§2.1/§6).
}
