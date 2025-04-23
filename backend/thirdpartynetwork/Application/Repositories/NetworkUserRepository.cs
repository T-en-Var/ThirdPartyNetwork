using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRepository(DataContext context, IIdentityService identityService, IIdentityVerifier identityVerifier) : BaseRepository(context), INetworkUserRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkUser> GetQueryable(Guid networkId)
        {
            var query = _context.NetworkUsers
                .AsQueryable();

            var currentUser = identityService.GetUserProxy(true);

            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadNetwork))
                query = query
                    .Include(x => x.Network);

            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadAccess))
                query = query
                    .Include(x => x.NetworkUserAccesses)
                    .ThenInclude(x => x.Access);

            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadUser))
                query = query
                    .Include(x => x.UserProxy);
            
            return query;
        }

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

        public ICollection<NetworkUser> GetByNetworkId(Guid networkId)
        {
            return [.. GetQueryable(networkId)
                .Where(nu => nu.NetworkId == networkId)];
        }

        public ICollection<NetworkUser> GetByUserProxyId(Guid id)
        {
            return [.. _context.NetworkUsers
                .Include(n => n.Network)
                .ThenInclude(x => x.NetworkUsers)
                .ThenInclude(x => x.UserProxy)
                .Where(nu => nu.UserProxyId == id)];
        }

        public NetworkUser? GetById(Guid networkId, Guid id)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(u => u.Id == id);
        }

        public NetworkUser? GetByNetworkIdAndUsername(Guid networkId, string username)
        {
            // Used for login, so always requires UserProxy
            return GetQueryable(networkId)
                .Include(n => n.UserProxy)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxy.Username == username);
        }

        public NetworkUser? GetByIds(Guid networkId, Guid userProxyId)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public NetworkUser Create(NetworkUser networkUser)
        {
            _context.NetworkUsers.Add(networkUser);
            Save();
            return networkUser;
        }

        public NetworkUser Update(Guid networkId, NetworkUser networkUser)
        {
            var existing = GetById(networkId, networkUser.Id);
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
