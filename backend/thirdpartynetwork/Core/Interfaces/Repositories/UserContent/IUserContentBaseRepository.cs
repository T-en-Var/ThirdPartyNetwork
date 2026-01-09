using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IUserContentBaseRepository<T> : IBaseRepository 
    where T : UserContentBaseModel
{
    Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid networkUserId, bool bypass = false);
    Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId);
}