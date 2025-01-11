using thirdpartynetwork.Application.Dtos.User;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        ICollection<User> GetAll();
        User? GetById(Guid id);
        User Create(CreateUserProxyDto createUser);
        User Update(Guid id, UpdateUserProxyDto updateUser);
        bool DeleteById(Guid id);
    }
}
