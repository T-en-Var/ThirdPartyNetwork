using thirdpartynetwork.Application.Dtos.UserProxy;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserProxyService : IBaseService
    {
        ICollection<UserProxy> GetAll();
        ICollection<UserProxy> GetByUserId();
        UserProxy? GetById(Guid id);
        UserProxy Create(CreateUserProxyDto createUserProxy);
        UserProxy Update(Guid id, UpdateUserProxyDto updateUserProxy);
        bool DeleteById(Guid id);
    }
}
