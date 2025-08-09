using thirdpartynetwork.Application.Dtos.User;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        ICollection<User> GetAll();
        User? GetById(Guid userId);
        User Create(CreateUserDto createUser);
        User Update(Guid userId, UpdateUserDto updateUser);
        void DeleteById(Guid userId);
    }
}
