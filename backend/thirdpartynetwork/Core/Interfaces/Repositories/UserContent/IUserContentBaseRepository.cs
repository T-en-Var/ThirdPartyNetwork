using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IUserContentBaseRepository<T> : IBaseRepository 
    where T : UserContentBaseModel
{
    ICollection<T> GetByAuthorId(Guid networkId, Guid networkUserId, bool bypass = false);
    ICollection<T> GetByUserProxyId(Guid userProxyId);
}