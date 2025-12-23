using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IAccessLevelVerifier
{
    bool CheckFileAccess(Guid fileId, UserProxy userProxy, Guid networkId);
    bool CheckCustomPageAccess(Guid customPageId, UserProxy userProxy, Guid networkId);
    bool CheckPageBlockAccess(Guid pageBlockId, UserProxy userProxy, Guid networkId);
    bool CheckConfigurationAccess(Guid configurationId, UserProxy userProxy, Guid networkId);
}