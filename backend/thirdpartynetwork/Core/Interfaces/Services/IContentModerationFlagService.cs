using thirdpartynetwork.Application.Dtos.ContentModerationFlag;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IContentModerationFlagService
{
    Task<ICollection<ContentModerationFlag>> GetForContentAsync(ContentKind kind, Guid contentId);
    Task<ContentModerationFlag> CreateAsync(Guid networkId, ContentKind kind, Guid contentId, CreateContentModerationFlagDto dto);
    Task<ContentModerationFlag> ResolveAsync(Guid networkId, Guid flagId, ResolveContentModerationFlagDto dto);
}
