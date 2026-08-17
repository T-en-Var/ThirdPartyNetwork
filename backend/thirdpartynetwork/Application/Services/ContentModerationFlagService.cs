using thirdpartynetwork.Application.Dtos.ContentModerationFlag;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Interfaces.Services;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

/// <summary>PLAN.md §2.5/§6: one shared service (parameterized by ContentKind) backs the
/// thin `.../{id}/moderation-flags` controller methods on all five content controllers.</summary>
public class ContentModerationFlagService(
    IContentModerationFlagRepository repository,
    IIdentityService identityService,
    INetworkUserService networkUserService) : IContentModerationFlagService
{
    public async Task<ICollection<ContentModerationFlag>> GetForContentAsync(ContentKind kind, Guid contentId)
    {
        return await repository.GetForContentAsync(kind, contentId);
    }

    public async Task<ContentModerationFlag> CreateAsync(Guid networkId, ContentKind kind, Guid contentId, CreateContentModerationFlagDto dto)
    {
        var userProxy = await identityService.GetUserProxyAsync();
        var networkUser = await networkUserService.GetByIdsAsync(networkId, userProxy.Id)
            ?? throw new NotFoundHttpException("NetworkUser not found");

        var flag = new ContentModerationFlag
        {
            Id = Guid.NewGuid(),
            ContentKind = kind,
            ContentId = contentId,
            NetworkId = networkId,
            FlaggedByNetworkUserId = networkUser.Id,
            Reason = dto.Reason,
            Status = Core.Enum.ModerationFlagStatus.Open,
            HiddenInNetwork = dto.HiddenInNetwork,
        };

        return await repository.CreateAsync(flag);
    }

    public async Task<ContentModerationFlag> ResolveAsync(Guid networkId, Guid flagId, ResolveContentModerationFlagDto dto)
    {
        var flag = await repository.GetByIdAsync(flagId) ?? throw new NotFoundHttpException("Moderation flag not found");
        if (flag.NetworkId != networkId) throw new NotFoundHttpException("Moderation flag not found");

        flag.Status = dto.Status;
        if (dto.HiddenInNetwork.HasValue)
        {
            flag.HiddenInNetwork = dto.HiddenInNetwork.Value;
        }

        return await repository.UpdateAsync(flag);
    }
}
