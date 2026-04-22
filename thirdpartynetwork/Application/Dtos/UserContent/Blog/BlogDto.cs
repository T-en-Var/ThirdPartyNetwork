using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Blog
{
    public class BlogDto : UserContentBaseDto
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Summary { get; set; }
        public FileLightDto? CoverImage { get; set; }
        public required JsonElement Body { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
    
    public class BlogLightDto : UserContentBaseLightDto
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Summary { get; set; }
        public FileLightDto? CoverImage { get; set; }
        public required JsonElement Body { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}