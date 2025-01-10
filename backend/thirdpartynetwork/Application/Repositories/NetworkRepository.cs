using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(DataContext context) : BaseRepository(context), INetworkRepository
    {
        private readonly DataContext _context = context;

        public Network? Create(Network network)
        {
            _context.Networks.Add(network);
            return !Save() ? null : network;
        }

        public bool Delete(Network network)
        {
            _context.Remove(network);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.Networks.Any(n => n.Id == id);
        }

        public ICollection<Network> GetAll()
        {
            return [.. _context.Networks];
        }

        public Network? GetById(Guid id)
        {
            return _context.Networks.Find(id);
        }

        public Network? GetByName(string name)
        {
            return  _context.Networks.Where(n => n.Name == name).FirstOrDefault();
        }

        public Network? Update(Network network)
        {
            Network? existing = this.GetById(network.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(network);

            if (!Save()) return null;
            return existing;
        }
    }
}
