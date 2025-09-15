namespace thirdpartynetwork.Core.Models;

public class File : BaseModel
{
    public Guid Id { get; set; }
    public required Guid PublicId { get; set; }
    public string Url { get; set; }
    public string? Format { get; set; }
    public long SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
    public string MediaType { get; set; } // e.g., image, video
    public string Name { get; set; }
    public required bool IsPublic { get; set; }
    public required NetworkUser Author { get; set; }
    public required Network Network { get; set; }
    public Guid NetworkId { get; set; }
}