using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class File : UserContentBaseModel
{
    public Guid Id { get; set; }
    public required Guid PublicId { get; set; }
    public string Url { get; set; }
    public string Format { get; set; }
    public long SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
    public string MediaType { get; set; }
    public string Name { get; set; }
    public bool IsSystemProtected { get; set; } = false;
}