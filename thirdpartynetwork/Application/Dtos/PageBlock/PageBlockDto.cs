using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.PageBlock;

public class PageBlockDto
{
    public Guid ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
    public Guid? CustomPageId { get; set; }
    public NetworkUserDto Author { get; set; }
}