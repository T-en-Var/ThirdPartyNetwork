using System.Text.Json;

namespace thirdpartynetwork.Core.Models;

public class Configuration : UserContentBaseModel
{
    public required string Key { get; set; }
    public required JsonDocument Value { get; set; }
}
