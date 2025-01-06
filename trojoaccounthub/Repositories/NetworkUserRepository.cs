using trojoaccounthub.Data;
using trojoaccounthub.Interfaces.Repositories;
using trojoaccounthub.Models;

namespace trojoaccounthub.Repositories
{
    public class NetworkUserRepository(DataContext _context) : BaseRepository(_context), INetworkUserRepository
    {
        public NetworkUser? Create(NetworkUser networkUser)
        {
            _context.NetworkUsers.Add(networkUser);
            if (!Save()) return null;
            return networkUser;
        }

        public bool Delete(NetworkUser networkUser)
        {
            _context.Remove(networkUser);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.NetworkUsers.Any(nu => nu.Id == id);
        }

        public ICollection<NetworkUser> GetAll()
        {
            return [.. _context.NetworkUsers];
        }

        public NetworkUser? GetById(Guid id)
        {
            return _context.NetworkUsers.Find(id);
        }

        public ICollection<NetworkUser> GetByNetworkId(Guid id)
        {
            return [.. _context.NetworkUsers.Where(nu => nu.NetworkId == id)];
        }

        public ICollection<NetworkUser> GetByUserProxyId(Guid id)
        {
            return [.. _context.NetworkUsers.Where(nu => nu.UserProxyId == id)];
        }

        public NetworkUser? Update(NetworkUser networkUser)
        {
            NetworkUser? existing = this.GetById(networkUser.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(networkUser);

            if (!Save()) return null;
            return existing;
        }
    }
}
