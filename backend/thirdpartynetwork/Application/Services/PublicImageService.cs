using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Interfaces.Services;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Services;

// Depends only on repositories (not IFileService/INetworkService) so it can be called from
// both UserProxyService and NetworkService without a circular DI dependency.
public class PublicImageService(
    IFileRepository fileRepository,
    ICDNService cdnService,
    IContentAccessGrantRepository contentAccessGrantRepository,
    INetworkRepository networkRepository) : IPublicImageService
{
    public async Task<File> MakePubliclyViewableAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdBypassingAccessAsync(fileId)
            ?? throw new NotFoundHttpException("File not found");

        if (file.Bucket != cdnService.PublicBucketName)
        {
            var newBucket = await cdnService.MoveAsync(file.ObjectKey, file.Bucket, cdnService.PublicBucketName);
            await fileRepository.UpdateBucketAsync(file, newBucket);
        }

        if (file.PublishedAt == null)
        {
            file.PublishedAt = DateTime.UtcNow;
            await fileRepository.SaveAndUpdateAsync(file);
        }

        var mainNetwork = await networkRepository.GetMainNetworkAsync();
        var existingGrants = await contentAccessGrantRepository.GetForContentAsync(ContentKind.File, file.Id);
        var alreadyPublic = existingGrants.Any(g =>
            g.GranteeType == GranteeType.Network && g.GranteeNetworkId == mainNetwork.Id && g.RevokedAt == null);

        if (!alreadyPublic)
        {
            await contentAccessGrantRepository.CreateAsync(new ContentAccessGrant
            {
                Id = Guid.NewGuid(),
                ContentKind = ContentKind.File,
                ContentId = file.Id,
                GranteeType = GranteeType.Network,
                GranteeNetworkId = mainNetwork.Id,
                Capabilities = ContentCapabilities.View,
            });
        }

        return file;
    }
}
