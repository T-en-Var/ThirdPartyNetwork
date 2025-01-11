using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(DataContext context) : BaseRepository(context), INetworkRepository
    {
        private readonly DataContext _context = context;

        public Network Create(Network network)
        {
            _context.Networks.Add(network);
            Save();
            return network;
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

        public Network Update(Network network)
        {
            var existing = this.GetById(network.Id);
            if (existing == null) throw new Exception("Network not found");

            _context.Entry(existing).CurrentValues.SetValues(network);

            Save();
            return existing;
        }
    }
}
