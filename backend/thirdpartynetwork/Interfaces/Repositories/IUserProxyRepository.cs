using thirdpartynetwork.Models;

namespace thirdpartynetwork.Interfaces.Repositories
{
    public interface IUserProxyRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<UserProxy> GetAll();
        ICollection<UserProxy> GetByUserId(Guid id);
        UserProxy? GetById(Guid id);
        UserProxy? Create(UserProxy userProxy);
        UserProxy? Update(UserProxy userProxy);
        bool Delete(UserProxy userProxy);
    }
}
