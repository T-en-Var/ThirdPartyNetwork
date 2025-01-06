using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IRoleRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<Role> GetAll();
        Role? GetById(int id);
        Role? Create(Role role);
        Role? Update(Role role);
        bool Delete(Role role);
    }
}
