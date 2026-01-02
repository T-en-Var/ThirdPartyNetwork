using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.UserContent.PageBlock;

public class UpdatePageBlockDto
{
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonElement Data { get; set; }
}