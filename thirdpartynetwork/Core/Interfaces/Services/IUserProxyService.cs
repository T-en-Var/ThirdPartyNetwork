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
        UserProxy? GetByUsername(string username);
        UserProxy Create(Guid userId, CreateUserProxyDto createUser);
        UserProxy Add(Guid userId, AddUserProxyDto addUserProxy);
        UserProxy Update(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUser);
        IQueryable<UserProxy> GetByIdQueryable(Guid userProxyId);
        void DeleteByIds(Guid userId, Guid userProxyId);
        // ICollection<Network> GetNetworksByIds(Guid userId, Guid userProxyId);
    }
}
