using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Configuration;

public class UpdateConfigurationDto : UpdateUserContentBaseDto
{
    public string? Key { get; set; }
    public JsonDocument? Value { get; set; }
}
