using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkAccessRepository(DataContext context) : BaseRepository(context), INetworkAccessRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid networkId, Guid accessId)
        {
            return _context.NetworkAccesses.Any(np => np.NetworkId == networkId && np.AccessId == accessId);
        }

        public ICollection<NetworkAccess> GetAll()
        {
            return [.. _context.NetworkAccesses
                .Include(n => n.Access)
                .Include(n => n.Network)];
        }

        public NetworkAccess? GetByIds(Guid networkId, Guid accessdId)
        {
            return _context.NetworkAccesses
                .Include(n => n.Access)
                .Include(n => n.Network)
                .FirstOrDefault(n => n.NetworkId == networkId && n.AccessId == accessdId);
        }

        public ICollection<NetworkAccess> GetByNetworkId(Guid id)
        {
            return [.. _context.NetworkAccesses
                .Where(np => np.NetworkId == id)
                .Include(n => n.Access)
                .Include(n => n.Network)];
        }

        public ICollection<NetworkAccess> GetByAccessId(Guid id)
        {
            return [.. _context.NetworkAccesses
                .Where(np => np.AccessId == id)
                .Include(n => n.Access)
                .Include(n => n.Network)];
        }

        public NetworkAccess Create(NetworkAccess networkAccess)
        {
            _context.NetworkAccesses.Add(networkAccess);
            Save();
            return networkAccess;
        }

        public NetworkAccess Update(NetworkAccess networkAccess)
        {
            var existing = this.GetByIds(networkAccess.NetworkId, networkAccess.AccessId);
            if (existing == null) throw new Exception("NetworkAccess not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");

            _context.Entry(existing).CurrentValues.SetValues(networkAccess);

            Save();
            return existing;
        }

        public bool Delete(NetworkAccess networkAccess)
        {
            if (networkAccess.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");
            
            _context.Remove(networkAccess);
            return Save();
        }
    }
}
