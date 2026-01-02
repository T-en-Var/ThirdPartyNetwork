using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Blog;

public class BlogDto : CustomUserContentBaseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Summary { get; set; }
    public FileDto? CoverImage { get; set; }
    public required JsonElement Body { get; set; }
    public DateTime? PublishedAt { get; set; }
}
