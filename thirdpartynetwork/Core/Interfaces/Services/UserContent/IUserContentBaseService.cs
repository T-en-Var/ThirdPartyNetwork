using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IUserContentBaseService<T> : IBaseService where T : UserContentBaseModel
{
    public ICollection<T> GetByAuthorId(Guid networkUserId);
    public ICollection<T> GetByUserProxyId(Guid userProxyId);
}