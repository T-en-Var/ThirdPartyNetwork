using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.Blog;

public class CreateBlogDto : CreateUserContentBaseDto
{
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public Guid? CoverImageId { get; set; }
    public required JsonElement Body { get; set; }
    public required AccessLevel AccessLevel { get; set; }
    public DateTime? PublishedAt { get; set; }
}
