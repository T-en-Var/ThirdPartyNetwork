using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.PageBlock;

public class UpdatePageBlockDto
{
    public Guid ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
}