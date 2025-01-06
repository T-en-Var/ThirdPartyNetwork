using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<User> GetAll();
        User? GetById(int id);
        User? Create(User user);
        User? Update(User user);
        bool Delete(User user);
    }
}
