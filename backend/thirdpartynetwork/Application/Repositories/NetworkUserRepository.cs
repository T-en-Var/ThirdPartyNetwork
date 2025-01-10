using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRepository(DataContext context) : BaseRepository(context), INetworkUserRepository
    {
        private readonly DataContext _context = context;

        public NetworkUser? Create(NetworkUser networkUser)
        {
            _context.NetworkUsers.Add(networkUser);
            return !Save() ? null : networkUser;
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
