using System.Text.Json;

namespace thirdpartynetwork.Core.Models;

public class Blog : UserContentBaseModel
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Summary { get; set; }
    public File? CoverImage { get; set; }
    public required JsonDocument Body { get; set; }
    public DateTime? PublishedAt { get; set; }
}
