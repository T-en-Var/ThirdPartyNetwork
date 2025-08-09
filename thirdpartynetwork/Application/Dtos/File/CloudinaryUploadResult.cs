namespace thirdpartynetwork.Application.Dtos.File;

public sealed class CloudinaryUploadResult
{
    public Guid Id { get; set; }
    public string SecureUrl { get; set; } = default!;
    public long Bytes { get; set; }
    public string ResourceType { get; set; } = default!;
    public string Format { get; set; } = default!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
}