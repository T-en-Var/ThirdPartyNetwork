using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Network> GetAll();
        Network? GetById(Guid id);
        Network? GetByName(string name);
        Network Create(Network network);
        Network Update(Network network);
        bool Delete(Network network);
    }
}
