using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IAccessRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Access> GetAll();
        Access? GetById(Guid id);
        Access Create(Access access);
        Access Update(Access access);
        void Delete(Access access);
    }
}
