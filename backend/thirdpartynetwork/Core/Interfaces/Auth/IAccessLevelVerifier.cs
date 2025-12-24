using System.Linq.Expressions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IAccessLevelVerifier
{
    bool CheckFileAccess(Guid fileId, Guid networkId, string permissionName);
    bool CheckCustomPageAccess(Guid customPageId, Guid networkId, string permissionName);
    bool CheckPageBlockAccess(Guid pageBlockId, Guid networkId, string permissionName);
    bool CheckConfigurationAccess(Guid configurationId, Guid networkId, string permissionName);
    bool CheckAccess(UserContentBaseModel model, Guid networkId, string permissionName);
    Expression<Func<T, bool>> GetAccessPredicate<T>(Guid networkId, string permissionName) where T : UserContentBaseModel;
}