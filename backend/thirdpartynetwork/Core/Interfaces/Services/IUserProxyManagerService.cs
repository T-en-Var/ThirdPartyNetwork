using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IUserProxyManagerService
{
    UserProxy AddUserProxy(Guid userId, AddUserProxyDto addUserProxy);
}