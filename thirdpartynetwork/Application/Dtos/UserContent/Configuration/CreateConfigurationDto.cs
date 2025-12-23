using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration;

public class CreateConfigurationDto : CreateUserContentBaseDto
{
    public required string Key { get; set; }
    public required JsonDocument Value { get; set; }
    public required AccessLevel AccessLevel { get; set; }
}
