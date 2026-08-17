namespace thirdpartynetwork.Application.Dtos.UserContent.File;

/// <summary>
/// PLAN.md §2.2: replaces CloudinaryUploadResult. No URL comes back from an upload — the
/// object's location is Bucket+ObjectKey, and a URL is only ever minted per-request
/// (ContentViewingLink / a stable public-bucket URL), never stored (§0.1 house rule:
/// "the API never proxies bytes", and no Url column anywhere per §2.2).
/// </summary>
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
