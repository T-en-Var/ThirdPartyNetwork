using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(DataContext context) : BaseRepository(context), INetworkRepository
    {
        private readonly DataContext _context = context;

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
            return  _context.Networks.FirstOrDefault(n => n.Name == name);
        }

        public Network Create(Network network)
        {
            _context.Networks.Add(network);
            Save();
            return network;
        }

        public Network Update(Network network)
        {
            var existing = this.GetById(network.Id);
            if (existing == null) throw new Exception("Network not found");

            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");

            _context.Entry(existing).CurrentValues.SetValues(network);

            Save();
            return existing;
        }

        public bool Delete(Network network)
        {
            if (network.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");
            
            _context.Remove(network);
            return Save();
        }
    }
}
