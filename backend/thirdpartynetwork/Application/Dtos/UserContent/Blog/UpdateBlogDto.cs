using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Blog;

public class UpdateBlogDto : UpdateUserContentBaseDto
{
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public Guid? CoverImageId { get; set; }
    public required JsonElement Body { get; set; }
}
