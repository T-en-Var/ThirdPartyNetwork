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

        public bool Exists(Guid networkUserId, Guid accessId)
        {
            return _context.NetworkUserAccesses.Any(nup =>  nup.NetworkUserId == networkUserId && nup.AccessId == accessId);
        }

        public ICollection<NetworkUserAccess> GetAll()
        {
            return [.. _context.NetworkUserAccesses
                .Include(n => n.NetworkUser)
                .Include(n => n.Access)];
        }

        public NetworkUserAccess? GetByIds(Guid networkUserId, Guid accessId)
        {
            return _context.NetworkUserAccesses
                .Include(n => n.NetworkUser)
                .Include(n => n.Access)
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.AccessId == accessId);
        }

        public ICollection<NetworkUserAccess> GetByNetworkUserId(Guid id)
        {
            return [.. _context.NetworkUserAccesses
                .Where(nup => nup.NetworkUserId == id)
                .Include(n => n.NetworkUser)
                .Include(n => n.Access)];
        }

        public ICollection<NetworkUserAccess> GetByAccessId(Guid accessId)
        {
            return [.. _context.NetworkUserAccesses
                .Where(nup => nup.AccessId == accessId)
                .Include(n => n.NetworkUser)
                .Include(n => n.Access)];
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

        public bool Delete(NetworkUserAccess networkUserAccess)
        {
            _context.Remove(networkUserAccess);
            return Save();
        }
    }
}
