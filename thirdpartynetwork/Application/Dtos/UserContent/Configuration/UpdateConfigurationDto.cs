using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration;

public class UpdateConfigurationDto : UpdateUserContentBaseDto
{
    public string? Key { get; set; }
    public JsonDocument? Value { get; set; }
    public required AccessLevel AccessLevel { get; set; }
}
