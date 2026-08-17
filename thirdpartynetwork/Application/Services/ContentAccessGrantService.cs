using thirdpartynetwork.Application.Dtos.ContentAccessGrant;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Interfaces.Services;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

/// <summary>
/// PLAN.md §2.4/§6: the one shared service parameterized by ContentKind backing the thin
/// `.../{id}/access` controller methods on all five content controllers.
/// </summary>
public class ContentAccessGrantService(
    IContentAccessGrantRepository repository,
    IIdentityService identityService,
    INetworkUserService networkUserService) : IContentAccessGrantService
{
    public async Task<ICollection<ContentAccessGrant>> GetForContentAsync(ContentKind kind, Guid contentId)
    {
        return await repository.GetForContentAsync(kind, contentId);
    }

    public async Task<ContentAccessGrant> CreateAsync(Guid networkId, ContentKind kind, Guid contentId, CreateContentAccessGrantDto dto)
    {
        ValidateGranteeShape(dto);

        var userProxy = await identityService.GetUserProxyAsync();
        var networkUser = await networkUserService.GetByIdsAsync(networkId, userProxy.Id)
            ?? throw new NotFoundHttpException("NetworkUser not found");

        var grant = new ContentAccessGrant
        {
            Id = Guid.NewGuid(),
            ContentKind = kind,
            ContentId = contentId,
            GranteeType = dto.GranteeType,
            GranteeNetworkId = dto.GranteeNetworkId,
            GranteeRoleId = dto.GranteeRoleId,
            GranteePermissionId = dto.GranteePermissionId,
            GranteeUserProxyId = dto.GranteeUserProxyId,
            Capabilities = dto.Capabilities,
            ExpiresAt = dto.ExpiresAt,
            GrantedByNetworkUserId = networkUser.Id,
        };

        return await repository.CreateAsync(grant);
    }

    public async Task RevokeAsync(Guid networkId, Guid grantId)
    {
        var grant = await repository.GetByIdAsync(grantId) ?? throw new NotFoundHttpException("Grant not found");
        await repository.RevokeAsync(grant);
    }

    // Each GranteeType names exactly one FK — reject a request that's ambiguous or missing
    // its target rather than silently creating a grant that can never match anyone (§2.4).
    private static void ValidateGranteeShape(CreateContentAccessGrantDto dto)
    {
        var required = dto.GranteeType switch
        {
            GranteeType.Network => dto.GranteeNetworkId.HasValue,
            GranteeType.NetworkRole => dto.GranteeNetworkId.HasValue && dto.GranteeRoleId.HasValue,
            GranteeType.NetworkPermission => dto.GranteeNetworkId.HasValue && dto.GranteePermissionId.HasValue,
            GranteeType.User => dto.GranteeUserProxyId.HasValue,
            _ => false,
        };

        if (!required)
            throw new BadHttpRequestException($"GranteeType {dto.GranteeType} requires its matching Grantee*Id field(s).");
    }
}
