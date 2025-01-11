using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IRoleRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Role> GetAll();
        Role? GetById(Guid id);
        Role Create(Role role);
        Role Update(Role role);
        bool Delete(Role role);
    }
}
