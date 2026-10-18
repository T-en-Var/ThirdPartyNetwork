namespace thirdpartynetwork.Application.Dtos.UserContent.File;

public sealed class CdnUploadResult
{
    public required string Bucket { get; set; }
    public required string ObjectKey { get; set; }
    public long Bytes { get; set; }
    public string ResourceType { get; set; } = default!;
    public string Format { get; set; } = default!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
}
