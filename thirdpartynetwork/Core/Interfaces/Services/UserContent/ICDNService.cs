using thirdpartynetwork.Application.Dtos.UserContent.File;

namespace thirdpartynetwork.Core.Interfaces.Services;

/// <summary>
/// PLAN.md §0.1/§5: the seam TPN talks to object storage through — MinioService is the
/// only implementation now (Cloudinary dropped, §0.1). Bucket/ObjectKey based, not
/// URL-based: no stable URL is ever produced or stored (§2.2) — resolving bytes for a
/// caller always goes through ContentViewingLink/GET /cdn/{token} (§2.3), which calls
/// GetPresignedDownloadUrlAsync/GetPublicUrl fresh, per request.
/// </summary>
public interface ICDNService
{
    Task<CdnUploadResult> UploadAsync(IFormFile file, Guid objectId, string? folder = null);

    Task DeleteAsync(string bucket, string objectKey);

    /// Moves an object between the public/private buckets (e.g. a File's AccessLevel/Tier
    /// changes from Private to Public or back) — MinIO has no atomic cross-bucket rename,
    /// so this is copy-then-remove. Returns the (possibly unchanged) resulting bucket name.
    Task<string> MoveAsync(string objectKey, string fromBucket, string toBucket);

    /// Short-lived, single-use-ish presigned GET — backs ContentViewingLink's
    /// Mode=OneTimeView/TimeLimited resolution (§2.3). A NEW signature every call.
    Task<Uri> GetPresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry);

    /// Stable, unsigned URL for the public bucket only (anonymous GET enabled there) —
    /// backs the Mode=Permanent / no-ContentViewingLink-row shortcut for content that's
    /// fully public (§2.3, §0.3).
    Uri GetPublicUrl(string bucket, string objectKey);

    /// Bucket names, exposed so callers (e.g. FileService reacting to an AccessLevel/Tier
    /// change) can decide a MoveAsync target without needing to know MinioOptions directly —
    /// ICDNService stays the one seam that knows about storage configuration.
    string PublicBucketName { get; }
    string PrivateBucketName { get; }

    // PLAN.md §2.3: Mode=OneTimeView/Permanent use the short default; Mode=TimeLimited
    // (streaming) uses the longer one — a single-render rule doesn't work for seekable media.
    TimeSpan DefaultPresignExpiry { get; }
    TimeSpan StreamingPresignExpiry { get; }

    /// One-time bucket bootstrap (PLAN.md §5) — called once at startup, not per-request.
    Task EnsureBucketsExistAsync();
}
