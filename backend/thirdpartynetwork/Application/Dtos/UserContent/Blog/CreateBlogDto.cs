using System.Text.Json;
using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.Blog;

public class CreateBlogDto : CreateUserContentBaseDto
{
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public Guid? CoverImageId { get; set; }
    public required JsonElement Body { get; set; }
    // AccessLevel/PublishedAt removed (§2.1/§6) — visibility now moves only through the
    // publish/unpublish actions and the ContentAccessGrant endpoints, never a field here.
}
