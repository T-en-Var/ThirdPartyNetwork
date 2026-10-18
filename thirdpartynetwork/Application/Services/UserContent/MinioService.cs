using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using thirdpartynetwork.Core.Helper;

namespace thirdpartynetwork.Application.Services;

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

        var targetBucket = _opts.PrivateBucket;

        await using var stream = file.OpenReadStream();

        await _internalClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(targetBucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType));

        var (mediaType, resourceType) = DetermineMediaType(file);

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
            Console.WriteLine($"MinIO delete failed for {bucket}/{objectKey} — but we don't care: {ex.Message}");
        }
    }

    public async Task ClearAllFilesAsync()
    {
        await ClearBucketAsync(_opts.PublicBucket);
        await ClearBucketAsync(_opts.PrivateBucket);
    }

    private async Task ClearBucketAsync(string bucket)
    {
        var exists = await _internalClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (!exists) return;

        var objectKeys = new List<string>();
        var listArgs = new ListObjectsArgs().WithBucket(bucket).WithRecursive(true);

        await foreach (var item in _internalClient.ListObjectsEnumAsync(listArgs))
        {
            if (!item.IsDir) objectKeys.Add(item.Key);
        }

        if (objectKeys.Count == 0) return;

        var removeArgs = new RemoveObjectsArgs().WithBucket(bucket).WithObjects(objectKeys);
        var errors = await _internalClient.RemoveObjectsAsync(removeArgs);
        foreach (var error in errors)
        {
            Console.WriteLine($"MinIO bulk delete failed for {bucket}/{error.Key}: {error.Message}");
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
