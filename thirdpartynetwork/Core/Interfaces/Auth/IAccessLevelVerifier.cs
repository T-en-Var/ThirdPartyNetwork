using System.Linq.Expressions;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IAccessLevelVerifier
{
    Task<bool> CheckFileAccessAsync(string fileId, Guid networkId, string permissionName);
    Task<bool> CheckCustomPageAccessAsync(string customPageId, Guid networkId, string permissionName);
    Task<bool> CheckPageBlockAccessAsync(string pageBlockId, Guid networkId, string permissionName);
    Task<bool> CheckConfigurationAccessAsync(string configurationId, Guid networkId, string permissionName);
    Task<bool> CheckBlogAccessAsync(string blogId, Guid networkId, string permissionName);
    Task<bool> CheckAccessAsync(UserContentBaseModel model, Guid networkId, string permissionName);

    // PLAN.md §2.3: same check, explicit caller identity — for ContentViewingLink
    // resolution, which is deliberately unauthenticated (§5).
    Task<bool> CheckAccessForCallerAsync(UserContentBaseModel? model, Guid networkId, Guid callerUserProxyId, string permissionName);

    Task<Expression<Func<T, bool>>> GetAccessPredicateAsync<T>(Guid networkId, string permissionName) where T : UserContentBaseModel;

    // PLAN.md §2.6: may `consumingNetworkId` durably adopt `content` (owned by a DIFFERENT
    // network) as its own — e.g. point a UserProxy/Network.ImageFileId or Blog.CoverImageId
    // at it? A grant naming consumingNetworkId specifically always wins; otherwise the
    // content must be reachable at all (published + some active grant) AND the owning
    // network's ExternalContentKinds must include this content's kind. Same network is
    // always allowed (not "external" at all).
    Task<bool> CanNetworkExternallyAdoptAsync(UserContentBaseModel content, ContentKind kind, Guid consumingNetworkId);
}