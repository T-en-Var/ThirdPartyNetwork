using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IPermissionRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Permission> GetAll();
        Permission? GetById(Guid id);
        Permission? Create(Permission permission);
        Permission? Update(Permission permission);
        bool Delete(Permission permission);
    }
}
