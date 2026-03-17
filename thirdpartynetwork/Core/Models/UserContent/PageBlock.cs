using System.Text.Json;
using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class PageBlock : UserContentBaseModel
{
    public Guid Id { get; set; }
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
    public Guid CustomPageId { get; set; }
    [JsonIgnore]
    public CustomPage CustomPage { get; set; }
}