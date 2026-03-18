using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.PageBlock
{
    public class PageBlockDto : CustomUserContentBaseDto
    {
        public Guid Id { get; set; }
        public Guid? ParentPageId { get; set; }
        public required string Text { get; set; }
        public required int Position { get; set; }
        public JsonElement Data { get; set; }
        public required Guid CustomPageId { get; set; }
        public required CustomPageLightDto CustomPage { get; set; }
    }
    
    public class PageBlockLightDto : CustomUserContentBaseLightDto
    {
        public Guid Id { get; set; }
        public Guid? ParentPageId { get; set; }
        public required string Text { get; set; }
        public required int Position { get; set; }
        public JsonElement Data { get; set; }
    }
}