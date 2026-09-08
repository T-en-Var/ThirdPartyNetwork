using thirdpartynetwork.Application.Dtos.UserContent.File;
using thirdpartynetwork.Core.Interfaces.Services;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// The authorization-gating tests in Tests/Integration don't exercise real object storage —
/// only whether a caller is allowed to reach the endpoint at all. Swapped in for the real
/// MinioService (which needs a live MinIO instance) via TestWebApplicationFactory.
/// </summary>
public class FakeCdnService : ICDNService
{
    public string PublicBucketName => "public";
    public string PrivateBucketName => "private";
    public TimeSpan DefaultPresignExpiry => TimeSpan.FromMinutes(15);
    public TimeSpan StreamingPresignExpiry => TimeSpan.FromHours(4);

    public Task<CdnUploadResult> UploadAsync(IFormFile file, Guid objectId, string? folder = null)
    {
        return Task.FromResult(new CdnUploadResult
        {
            Bucket = PrivateBucketName,
            ObjectKey = $"{folder ?? "test"}/{objectId}",
            Bytes = file.Length,
            Format = Path.GetExtension(file.FileName).TrimStart('.'),
            ResourceType = file.ContentType,
        });
    }

    public Task DeleteAsync(string bucket, string objectKey) => Task.CompletedTask;

    public Task<string> MoveAsync(string objectKey, string fromBucket, string toBucket) => Task.FromResult(objectKey);

    public Task<Uri> GetPresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry) =>
        Task.FromResult(new Uri($"https://cdn.test.local/{bucket}/{objectKey}?presigned=1"));

    public Uri GetPublicUrl(string bucket, string objectKey) =>
        new($"https://cdn.test.local/{bucket}/{objectKey}");

    public Task EnsureBucketsExistAsync() => Task.CompletedTask;
}
