using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using thirdpartynetwork.Application.Dtos.UserContent.File;
using thirdpartynetwork.Core.Helper;

namespace thirdpartynetwork.Application.Services;

/// <summary>
/// PLAN.md §0.1/§5: replaces CloudinaryService as the sole ICDNService implementation.
/// Two clients, deliberately — one for actual object operations (talks to MinIO over the
/// internal Docker network), one used ONLY to mint presigned URLs (must be built against the
/// external/public endpoint, because a presigned URL's signature covers the Host header the
/// caller who receives it will actually hit — signing with the internal hostname would
/// produce a URL that 403s the moment it's resolved from outside the Docker network).
/// </summary>
public sealed class MinioService : ICDNService
{
    private readonly MinioOptions _opts;
    private readonly IMinioClient _internalClient;
    private readonly IMinioClient _presignClient;

    public MinioService(IOptions<MinioOptions> options)
    {
        _opts = options.Value;

        _internalClient = new MinioClient()
            .WithEndpoint(_opts.InternalEndpoint)
            .WithCredentials(_opts.RootUser, _opts.RootPassword)
            .WithSSL(_opts.InternalUseSsl)
            .Build();

        _presignClient = new MinioClient()
            .WithEndpoint(_opts.ExternalEndpoint)
            .WithCredentials(_opts.RootUser, _opts.RootPassword)
            .WithSSL(_opts.ExternalUseSsl)
            .Build();
    }

    /// One-time bucket bootstrap (PLAN.md §5): creates the public/private buckets if
    /// missing, and enables anonymous GET only on the public one. Called once at startup
    /// (Program.cs), not per-request.
    public async Task EnsureBucketsExistAsync()
    {
        await EnsureBucketAsync(_opts.PrivateBucket, anonymousRead: false);
        await EnsureBucketAsync(_opts.PublicBucket, anonymousRead: true);
    }

    private async Task EnsureBucketAsync(string bucket, bool anonymousRead)
    {
        var exists = await _internalClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (!exists)
        {
            await _internalClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
        }

        if (anonymousRead)
        {
            var policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": "*",
                      "Action": ["s3:GetObject"],
                      "Resource": ["arn:aws:s3:::{{bucket}}/*"]
                    }
                  ]
                }
                """;

            await _internalClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(bucket).WithPolicy(policy));
        }
    }

    public async Task<CdnUploadResult> UploadAsync(IFormFile file, Guid objectId, string? folder = null)
    {
        if (file.Length == 0) throw new ArgumentException("File is empty.", nameof(file));

        var ext = Path.GetExtension(file.FileName)?.TrimStart('.').ToLowerInvariant();
        var objectKey = string.IsNullOrEmpty(folder)
            ? objectId.ToString()
            : $"{folder}/{objectId}";
        if (!string.IsNullOrEmpty(ext)) objectKey += $".{ext}";

        // New content is always private on upload (matches the pre-existing default of
        // AccessLevel.Private, §2.1/§2.2) — moving to the public bucket happens later, via
        // MoveAsync, only once the content's tier/AccessLevel actually changes.
        var targetBucket = _opts.PrivateBucket;

        await using var stream = file.OpenReadStream();

        await _internalClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(targetBucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType));

        var (mediaType, resourceType) = DetermineMediaType(file);

        // MinIO is dumb object storage — unlike Cloudinary, it doesn't derive image/video
        // dimensions or duration server-side. Width/Height/Duration are left null here; a
        // real deployment would want a probing step (e.g. an image-metadata library, or
        // ffprobe for video) as a follow-up. Not done here — out of scope for the storage
        // swap itself, and PLAN.md §8 already earmarks a `worker` service for exactly this
        // class of post-processing.
        return new CdnUploadResult
        {
            Bucket = targetBucket,
            ObjectKey = objectKey,
            Bytes = file.Length,
            ResourceType = resourceType,
            Format = string.IsNullOrEmpty(ext) ? mediaType : ext,
            Width = null,
            Height = null,
            Duration = null,
        };
    }

    public async Task DeleteAsync(string bucket, string objectKey)
    {
        try
        {
            await _internalClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucket).WithObject(objectKey));
        }
        catch (Minio.Exceptions.MinioException ex)
        {
            // Mirrors the previous CloudinaryService behaviour: a delete for an object that's
            // already gone shouldn't blow up the caller (e.g. DatabaseSeeder's clean-sweep).
            Console.WriteLine($"MinIO delete failed for {bucket}/{objectKey} — but we don't care: {ex.Message}");
        }
    }

    public async Task<string> MoveAsync(string objectKey, string fromBucket, string toBucket)
    {
        if (fromBucket == toBucket) return toBucket;

        await _internalClient.CopyObjectAsync(new CopyObjectArgs()
            .WithBucket(toBucket)
            .WithObject(objectKey)
            .WithCopyObjectSource(new CopySourceObjectArgs()
                .WithBucket(fromBucket)
                .WithObject(objectKey)));

        await _internalClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(fromBucket).WithObject(objectKey));

        return toBucket;
    }

    public async Task<Uri> GetPresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry)
    {
        var url = await _presignClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds));

        return new Uri(url);
    }

    public Uri GetPublicUrl(string bucket, string objectKey)
    {
        var scheme = _opts.ExternalUseSsl ? "https" : "http";
        return new Uri($"{scheme}://{_opts.ExternalEndpoint}/{bucket}/{objectKey}");
    }

    public string PublicBucketName => _opts.PublicBucket;
    public string PrivateBucketName => _opts.PrivateBucket;
    public TimeSpan DefaultPresignExpiry => TimeSpan.FromSeconds(_opts.PresignExpirySecondsDefault);
    public TimeSpan StreamingPresignExpiry => TimeSpan.FromSeconds(_opts.PresignExpirySecondsStreaming);

    private static (string mediaType, string resourceType) DetermineMediaType(IFormFile file)
    {
        var ct = file.ContentType?.ToLowerInvariant() ?? "";

        if (ct.StartsWith("image/")) return (ct, "image");
        if (ct.StartsWith("video/")) return (ct, "video");
        if (ct.StartsWith("audio/")) return (ct, "audio");

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
        if (IsImageExt(ext)) return ("image", "image");
        if (IsVideoOrAudioExt(ext)) return ("video", "video");

        return ("raw", "raw");
    }

    private static bool IsImageExt(string ext) =>
        ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".avif" or ".bmp" or ".tiff";

    private static bool IsVideoOrAudioExt(string ext) =>
        ext is ".mp4" or ".mov" or ".webm" or ".mkv" or ".avi" or ".mp3" or ".wav" or ".aac" or ".ogg" or ".flac";
}
