using System.Text.Json;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.PageBlock;

public class UpdatePageBlockDto
{
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
}