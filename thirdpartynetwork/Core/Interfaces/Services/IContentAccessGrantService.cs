using thirdpartynetwork.Application.Dtos.ContentAccessGrant;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IContentAccessGrantService
{
    Task<ICollection<ContentAccessGrant>> GetForContentAsync(ContentKind kind, Guid contentId);
    Task<ContentAccessGrant> CreateAsync(Guid networkId, ContentKind kind, Guid contentId, CreateContentAccessGrantDto dto);
    Task RevokeAsync(Guid networkId, Guid grantId);
}
