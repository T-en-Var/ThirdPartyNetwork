using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.PageBlock;

public class PageBlockDto : CustomUserContentBaseDto
{
    public Guid Id { get; set; }
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
    public Guid? CustomPageId { get; set; }
}