using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRepository(DataContext context) : BaseRepository(context), INetworkUserRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid id)
        {
            return _context.NetworkUsers.Any(nu => nu.Id == id);
        }

        public ICollection<NetworkUser> GetAll()
        {
            return [.. _context.NetworkUsers
                .Include(n => n.Network)
                .Include(n => n.UserProxy)];
        }

        public ICollection<NetworkUser> GetByNetworkId(Guid id)
        {
            return [.. _context.NetworkUsers
                .Where(nu => nu.NetworkId == id)
                .Include(n => n.Network)
                .Include(n => n.UserProxy)];
        }

        public ICollection<NetworkUser> GetByUserProxyId(Guid id)
        {
            return [.. _context.NetworkUsers
                .Where(nu => nu.UserProxyId == id)
                .Include(n => n.Network)
                .Include(n => n.UserProxy)];
        }

        public NetworkUser? GetById(Guid id)
        {
            return _context.NetworkUsers
                .Include(n => n.Network)
                .Include(n => n.UserProxy)
                .FirstOrDefault(u => u.Id == id);
        }

        public NetworkUser? GetByIds(Guid networkId, Guid userProxyId)
        {
            return _context.NetworkUsers
                .Include(n => n.Network)
                .Include(n => n.UserProxy)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public NetworkUser Create(NetworkUser networkUser)
        {
            _context.NetworkUsers.Add(networkUser);
            Save();
            return networkUser;
        }

        public NetworkUser Update(NetworkUser networkUser)
        {
            var existing = GetById(networkUser.Id);
            if (existing == null) throw new Exception("NetworkUser not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUser);

            Save();
            return existing;
        }

        public bool Delete(NetworkUser networkUser)
        {
            _context.Remove(networkUser);
            return Save();
        }
    }
}
