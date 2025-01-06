using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IPermissionRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<Permission> GetAll();
        Permission? GetById(int id);
        Permission? Create(Permission permission);
        Permission? Update(Permission permission);
        bool Delete(Permission permission);
    }
}
