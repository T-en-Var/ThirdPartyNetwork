using thirdpartynetwork.Application.Dtos.User;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        ICollection<User> GetAll();
        User? GetById(Guid id);
        User Create(CreateUserProxyDto user);
        User Update(Guid id, UpdateUserProxyDto user);
        bool DeleteById(Guid id);
    }
}
