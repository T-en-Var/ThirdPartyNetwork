using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Network> GetAll();
        Network? GetById(Guid id);
        Network GetMainNetwork();
        Network? GetByName(string name);
        IQueryable<Network> GetByNameQueryable(string name);
        IQueryable<Network> GetByIdQueryable(Guid id);
        Network Create(Network network);
        Network Update(Network network);
        bool Delete(Network network);
    }
}
