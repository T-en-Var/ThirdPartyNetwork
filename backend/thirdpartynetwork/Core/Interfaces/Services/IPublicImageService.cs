using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IPublicImageService
{
    // Moves the file into the public bucket, publishes it, and ensures an anonymous-view grant
    // to the Main (IsPublic) network exists, so it has a stable, always-reachable public URL.
    Task<File> MakePubliclyViewableAsync(Guid fileId);
}
