namespace thirdpartynetwork.Core.Models;

public class File : UserContentBaseModel
{
    // PLAN.md §2.2: Bucket + ObjectKey replace Cloudinary's PublicId/Url pair. No Url
    // column anywhere — URLs are never stored, only minted per-request (ContentViewingLink,
    // §2.3). ObjectKey is unique within a bucket, so (Bucket, ObjectKey) is the storage
    // location; Bucket changes (private <-> public) via ICDNService.MoveAsync when the
    // file's tier/AccessLevel changes.
    public required string Bucket { get; set; }
    public required string ObjectKey { get; set; }

    public string Format { get; set; }
    public long SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
    public string MediaType { get; set; }
    public string Name { get; set; }
    public bool IsSystemProtected { get; set; } = false;
}
