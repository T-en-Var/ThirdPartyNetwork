using thirdpartynetwork.Application.Dtos.UserProxy;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserProxyService : IBaseService
    {
        ICollection<UserProxy> GetAll();
        ICollection<UserProxy> GetByUserId(Guid userId);
        UserProxy? GetById(Guid userProxyId);
        UserProxy? GetByIds(Guid userId, Guid userProxyId);
        UserProxy Create(Guid userId, CreateUserProxyDto createUser);
        UserProxy Update(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUser);
        bool DeleteByIds(Guid userId, Guid userProxyId);
    }
}
