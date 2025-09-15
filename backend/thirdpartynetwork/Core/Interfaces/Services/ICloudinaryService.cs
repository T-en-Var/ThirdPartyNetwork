using CloudinaryDotNet.Actions;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface ICDNService
{
    Task<CloudinaryUploadResult> UploadAsync(
        IFormFile file,
        Guid publicId,
        string? folder = null);

    Task DeleteAsync(Guid publicId, ResourceType resourceType);
}