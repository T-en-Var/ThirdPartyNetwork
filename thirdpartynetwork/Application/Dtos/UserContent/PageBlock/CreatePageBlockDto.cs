using System.Text.Json;
using System.Text.Json.Nodes;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.PageBlock;

public class CreatePageBlockDto : CreateUserContentBaseDto
{
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonElement Data { get; set; }
}