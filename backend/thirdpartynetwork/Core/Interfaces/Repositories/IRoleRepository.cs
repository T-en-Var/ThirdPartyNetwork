using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IRoleRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Role> GetAll();
        ICollection<Role> GetByNetworkId(Guid networkId);
        Role? GetById(Guid networkId, Guid roleId);
        Role Create(Role role);
        Role Update(Guid networkId, Role role);
        bool Delete(Role role);
    }
}
