namespace thirdpartynetwork.Core.Helper;

/// <summary>
/// PLAN.md §5: MinIO runs as a peer container to TPN. Two endpoints are configured
/// deliberately, not one — presigned URLs are signed against a Host header, so a URL
/// signed for the internal Docker-network hostname would fail signature validation when
/// resolved by an external caller against the public hostname, and vice versa
/// (FILE_SERVICE_SETUP.md's documented presign/Host-header gotcha). MinioService keeps
/// one client per endpoint for exactly this reason (see MinioService.cs).
/// </summary>
public sealed class MinioOptions
{
    public string RootUser { get; set; } = default!;
    public string RootPassword { get; set; } = default!;

    /// Docker-internal endpoint TPN uses for actual object operations (put/get/remove/bucket
    /// management) — e.g. "minio:9000".
    public string InternalEndpoint { get; set; } = default!;
    public bool InternalUseSsl { get; set; } = false;

    /// Public-facing endpoint (behind nginx) that presigned URLs must be signed for, since
    /// that's the Host header the caller who receives the URL will actually hit — e.g.
    /// "cdn.example.com".
    public string ExternalEndpoint { get; set; } = default!;
    public bool ExternalUseSsl { get; set; } = true;

    /// Anonymous GET enabled — backs AccessLevel.Public / Tier()=PublicToNetwork-via-
    /// IsPublic-network content, per PLAN.md §2.3 ("Mode=Permanent... returns a stable URL
    /// straight at MinIO's public bucket").
    public string PublicBucket { get; set; } = "public";

    /// No anonymous access — everything else goes through a ContentViewingLink-minted
    /// presigned URL (PLAN.md §2.3).
    public string PrivateBucket { get; set; } = "private";

    /// Default presign expiry for a single render (PLAN.md §2.3 Mode=OneTimeView).
    public int PresignExpirySecondsDefault { get; set; } = 300;

    /// Longer expiry for seekable media (PLAN.md §2.3 Mode=TimeLimited), matching
    /// FILE_SERVICE_SETUP.md §4.4's PRESIGN_EXPIRY_STREAMING_SECONDS reasoning.
    public int PresignExpirySecondsStreaming { get; set; } = 21600;
}
