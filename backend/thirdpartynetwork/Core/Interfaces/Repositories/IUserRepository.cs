using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<User> GetAll();
        User? GetById(Guid id);
        User Create(User user);
        User Update(User user);
        bool Delete(User user);
    }
}
