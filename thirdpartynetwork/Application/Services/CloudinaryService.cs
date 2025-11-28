using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using thirdpartynetwork.Application.Helpers;
using thirdpartynetwork.Core.Helper;

namespace thirdpartynetwork.Application.Services;

public sealed class CloudinaryService : ICDNService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _opts;

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
        _opts = options.Value;
        _cloudinary = new Cloudinary(new Account(_opts.CloudName, _opts.ApiKey, _opts.ApiSecret))
        {
            Api =
            {
                Secure = true,
                Timeout = 60000,  // 1 minute
            }
        };
    }

    public async Task<CloudinaryUploadResult> UploadAsync(
        IFormFile file, 
        Guid id,
        string? folder = null)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty.", nameof(file));

        var kind = DetermineKind(file);
        var ext = Path.GetExtension(file.FileName)?.TrimStart('.').ToLowerInvariant();
    
        folder ??= _opts.DefaultFolder;
    
        // Copy to MemoryStream to decouple from the incoming Request Stream
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset position to start

        var progress = new Progress<double>(p =>
        {
            // This creates noise in logs
            // Console.WriteLine($"Upload progress: {p:P1}"); 
        });

        // Wrap the memory stream, not the file stream
        await using var progressStream = new ProgressStream(memoryStream, progress);
        RawUploadResult result;
        
        switch (kind)
        {
            case MediaKind.Image:
            {
                var p = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, progressStream),
                    Folder = folder,
                    PublicId = id.ToString(),
                    Overwrite = true,
                    UseFilename = Guid.Empty == id,
                    UniqueFilename = true,
                };

                result = await _cloudinary.UploadAsync(p).ConfigureAwait(false);
                break;
            }

            case MediaKind.VideoOrAudio:
            {
                var p = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, progressStream),
                    Folder = folder,
                    PublicId = id.ToString(),
                    Overwrite = true,
                    UseFilename = Guid.Empty == id,
                    UniqueFilename = true,
                };

                // For very large files you can switch to UploadLargeAsync(p)
                result = await _cloudinary.UploadAsync(p).ConfigureAwait(false);
                break;
            }

            case MediaKind.Raw:
            default: 
            {
                var p = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, progressStream),
                    Folder = folder,
                    PublicId = id.ToString(),
                    Overwrite = true,
                    UseFilename = Guid.Empty == id,
                    UniqueFilename = true,
                };

                result = await _cloudinary.UploadAsync(p).ConfigureAwait(false);
                break;
            }
        }

        if (result.Error != null)
            throw new InvalidOperationException(result.Error.Message);
        
        return MapUploadResult(result, ext);
    }

    public async Task DeleteAsync(string publicId, string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("Resource type must be provided", nameof(resourceType));

        resourceType = resourceType.ToLowerInvariant();

        var deletionParams = new DeletionParams(publicId)
        {
            Invalidate = true,
            ResourceType = resourceType switch
            {
                "image" => ResourceType.Image,
                "video" => ResourceType.Video,
                "raw"   => ResourceType.Raw,
                _       => throw new NotSupportedException($"Unsupported resource type: {resourceType}")
            }
        };

        var result = await _cloudinary.DestroyAsync(deletionParams).ConfigureAwait(false);

        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary error: {result.Error.Message}");

        if (string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"File not found in Cloudinary [{resourceType}]: {publicId} - But we don't care.");
            // throw new FileNotFoundException($"File not found in Cloudinary [{resourceType}]: {publicId}");
    }

    
    private static CloudinaryUploadResult MapUploadResult(RawUploadResult r, string? originalExt = null)
    {
        int? width = null;
        int? height = null;
        TimeSpan? duration = null;

        switch (r)
        {
            case ImageUploadResult img:
                width = img.Width;
                height = img.Height;
                break;

            case VideoUploadResult vid:
                width = vid.Width;
                height = vid.Height;
                if (vid.Duration is var seconds)
                {
                    duration = TimeSpan.FromSeconds(seconds);
                }
                break;
        }

        var path = r.PublicId.Split('/').ToList();
        var guidPart = Path.GetFileNameWithoutExtension(path[^1]);

        if (!Guid.TryParse(guidPart, out var id))
            throw new FormatException($"Invalid GUID in PublicId: {r.PublicId}");

        var format = r.Format;
        if (string.IsNullOrWhiteSpace(format))
        {
            if (!string.IsNullOrWhiteSpace(originalExt))
            {
                format = originalExt;
            }
            else if (!string.IsNullOrWhiteSpace(r.ResourceType))
            {
                format = r.ResourceType.ToLowerInvariant();
            }
        }
    
        return new CloudinaryUploadResult
        {
            Id = id,
            SecureUrl = r.SecureUrl?.ToString() ?? string.Empty,
            Bytes = r.Bytes,
            ResourceType = r.ResourceType,
            Format = format ?? "unknown",
            Width = width,
            Height = height,
            Duration = duration
        };
    }

    private static MediaKind DetermineKind(IFormFile file)
    {
        // Prefer MIME type:
        var ct = file.ContentType?.ToLowerInvariant() ?? "";

        if (ct.StartsWith("image/"))
            return MediaKind.Image;

        if (ct.StartsWith("video/") || ct.StartsWith("audio/"))
            return MediaKind.VideoOrAudio;

        // Fallback to extension
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
        if (IsImageExt(ext)) return MediaKind.Image;
        if (IsVideoOrAudioExt(ext)) return MediaKind.VideoOrAudio;

        return MediaKind.Raw;
    }

    private static bool IsImageExt(string ext) => ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".avif" or ".bmp" or ".tiff";

    private static bool IsVideoOrAudioExt(string ext) => ext is ".mp4" or ".mov" or ".webm" or ".mkv" or ".avi" or ".mp3" or ".wav" or ".aac" or ".ogg" or ".flac";
}

internal enum MediaKind
{
    Image,
    VideoOrAudio,
    Raw
}