using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IUserContentBaseService<T> : IBaseService where T : UserContentBaseModel
{
    public Task<ICollection<T>> GetByAuthorIdAsync(Guid networkUserId);
    public Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId);
    Task<T> PublishAsync(T entity);
    Task<T> UnpublishAsync(T entity);
    Task<T> SetOwnershipModeAsync(T entity, OwnershipMode mode);
}
