using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserAccessRepository(DataContext context) : BaseRepository(context), INetworkUserAccessRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkUserAccess> GetQueryable()
        {
            var query = _context.NetworkUserAccesses
                .Include(n => n.NetworkUser)
                .Include(n => n.Access)
                .AsQueryable();
            
            return query;
        }
        
        public bool Exists(Guid networkUserId, Guid accessId)
        {
            return _context.NetworkUserAccesses
                .Any(nup =>  nup.NetworkUserId == networkUserId && nup.AccessId == accessId);
        }

        public ICollection<NetworkUserAccess> GetAll()
        {
            return [.. GetQueryable()];
        }

        public void Populate(Guid networkId, Guid accessId)
        {
            var networkUsers = _context.NetworkUsers
                .Where(u => u.NetworkId == networkId)
                .ToList();

            var newAccesses = networkUsers.Select(nu => new NetworkUserAccess
            {
                NetworkUserId = nu.Id,
                AccessId = accessId,
                IsAccepted = false,
                NetworkUser = null,
                Access = null
            }).ToList();

            _context.NetworkUserAccesses.AddRange(newAccesses);
            Save();
        }

        public void Depopulate(Guid networkId, Guid accessId)
        {
            var accessesToRemove = _context.NetworkUserAccesses
                .Where(nua => nua.AccessId == accessId && nua.NetworkUser.NetworkId == networkId)
                .ToList();

            _context.NetworkUserAccesses.RemoveRange(accessesToRemove);
            Save();
        }

        public NetworkUserAccess? GetByIds(Guid networkUserId, Guid accessId)
        {
            return GetQueryable()
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.AccessId == accessId);
        }

        public ICollection<NetworkUserAccess> GetByNetworkUserId(Guid id)
        {
            return [.. GetQueryable()
                .Where(nup => nup.NetworkUserId == id)];
        }

        public ICollection<NetworkUserAccess> GetByAccessId(Guid accessId)
        {
            return [.. GetQueryable()
                .Where(nup => nup.AccessId == accessId)];
        }

        public NetworkUserAccess Create(NetworkUserAccess networkUserPermission)
        {
            _context.NetworkUserAccesses.Add(networkUserPermission);
            Save();
            return networkUserPermission;
        }

        public NetworkUserAccess Update(NetworkUserAccess networkUserAccess)
        {
            var existing = GetByIds(networkUserAccess.NetworkUserId, networkUserAccess.AccessId);
            if (existing == null) throw new Exception("NetworkUserPermission not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserAccess);

            Save();
            return existing;
        }

        public void MarkAccessComplete(Guid networkUserId)
        {
            // Fast check: are there any required accesses that are NOT accepted?
            bool hasUnacceptedRequiredAccess = _context.NetworkAccesses
                .Where(na => na.IsRequired && na.Network.NetworkUsers.Any(nu => nu.Id == networkUserId))
                .Any(na => !_context.NetworkUserAccesses
                    .Any(nua => nua.NetworkUserId == networkUserId && nua.AccessId == na.AccessId && nua.IsAccepted));

            if (hasUnacceptedRequiredAccess)
                return;

            var networkUser = _context.NetworkUsers.FirstOrDefault(nu => nu.Id == networkUserId);
            if (networkUser == null)
                return;
            
            Console.WriteLine("User complies");

            networkUser.AccessIncomplete = false;
            _context.SaveChanges();
        }

        public bool Delete(NetworkUserAccess networkUserAccess)
        {
            _context.Remove(networkUserAccess);
            return Save();
        }
    }
}
