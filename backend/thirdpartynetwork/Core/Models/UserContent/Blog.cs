using System.Text.Json;

namespace thirdpartynetwork.Core.Models;

public class Blog : UserContentBaseModel
{
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Summary { get; set; }
    public File? CoverImage { get; set; }
    public Guid? CoverImageId { get; set; }
    public required JsonDocument Body { get; set; }
    // PublishedAt moved to UserContentBaseModel (§2.1) — it's no longer Blog-only.
}
