using thirdpartynetwork.Application.Dtos.UserContent.File;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface ICDNService
{
    Task<CdnUploadResult> UploadAsync(IFormFile file, Guid objectId, string? folder = null);

    Task DeleteAsync(string bucket, string objectKey);
    Task ClearAllFilesAsync();
    Task<string> MoveAsync(string objectKey, string fromBucket, string toBucket);
    Task<Uri> GetPresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry);
    Uri GetPublicUrl(string bucket, string objectKey);
    string PublicBucketName { get; }
    string PrivateBucketName { get; }
    TimeSpan DefaultPresignExpiry { get; }
    TimeSpan StreamingPresignExpiry { get; }
    Task EnsureBucketsExistAsync();
}
