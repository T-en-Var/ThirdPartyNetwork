using System.Linq.Expressions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IAccessLevelVerifier
{
    bool CheckFileAccess(string fileId, Guid networkId, string permissionName);
    bool CheckCustomPageAccess(string customPageId, Guid networkId, string permissionName);
    bool CheckPageBlockAccess(string pageBlockId, Guid networkId, string permissionName);
    bool CheckConfigurationAccess(string configurationId, Guid networkId, string permissionName);
    bool CheckBlogAccess(string blogId, Guid networkId, string permissionName);
    bool CheckAccess(UserContentBaseModel model, Guid networkId, string permissionName);
    Expression<Func<T, bool>> GetAccessPredicate<T>(Guid networkId, string permissionName) where T : UserContentBaseModel;
}