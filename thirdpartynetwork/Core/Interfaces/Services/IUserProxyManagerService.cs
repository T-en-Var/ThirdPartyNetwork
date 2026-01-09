using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IUserProxyManagerService
{
    Task<UserProxy> AddUserProxyAsync(Guid userId, AddUserProxyDto addUserProxy);
}