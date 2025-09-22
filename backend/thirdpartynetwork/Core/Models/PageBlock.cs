using System.Text.Json;

namespace thirdpartynetwork.Core.Models;

public class PageBlock : BaseModel
{
    public Guid Id { get; set; }
    public Guid? ParentPageId { get; set; }
    public required string Text { get; set; }
    public required int Position { get; set; }
    public JsonDocument Data { get; set; }
    public Guid CustomPageId { get; set; }
    public CustomPage CustomPage { get; set; }
    public Guid AuthorId { get; set; }
    public NetworkUser Author { get; set; }
}