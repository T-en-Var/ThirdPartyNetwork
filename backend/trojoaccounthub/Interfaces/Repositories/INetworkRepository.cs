using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<Network> GetAll();
        Network? GetById(int id);
        Network? GetByName(string name);
        Network? Create(Network network);
        Network? Update(Network network);
        bool Delete(Network network);
    }
}
