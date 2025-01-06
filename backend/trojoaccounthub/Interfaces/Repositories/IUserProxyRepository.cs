using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IUserProxyRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<UserProxy> GetAll();
        ICollection<UserProxy> GetByUserId(int id);
        UserProxy? GetById(int id);
        UserProxy? Create(UserProxy userProxy);
        UserProxy? Update(UserProxy userProxy);
        bool Delete(UserProxy userProxy);
    }
}
