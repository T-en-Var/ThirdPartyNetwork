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
    Task<bool> CheckAccessForCallerAsync(UserContentBaseModel? model, Guid networkId, Guid callerUserProxyId, string permissionName);
    Task<Expression<Func<T, bool>>> GetAccessPredicateAsync<T>(Guid networkId, string permissionName) where T : UserContentBaseModel;
    Task<Expression<Func<T, bool>>> GetCrossNetworkAccessPredicateAsync<T>(string permissionName) where T : UserContentBaseModel;
    Task<bool> CanNetworkExternallyAdoptAsync(UserContentBaseModel content, ContentKind kind, Guid consumingNetworkId);
}