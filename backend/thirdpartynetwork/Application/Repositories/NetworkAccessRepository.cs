using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkAccessRepository(DataContext context) : BaseRepository(context), INetworkAccessRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkAccess> GetQueryable()
        {
            var query = _context.NetworkAccesses
                .Include(n => n.Access)
                .Include(n => n.Network)
                .AsQueryable();
            
            return query;
        }

        public bool Exists(Guid networkId, Guid accessId)
        {
            return _context.NetworkAccesses.Any(np => np.NetworkId == networkId && np.AccessId == accessId);
        }

        public ICollection<NetworkAccess> GetAll()
        {
            return [.. GetQueryable()];
        }

        public NetworkAccess? GetByIds(Guid networkId, Guid accessdId)
        {
            return GetQueryable()
                .FirstOrDefault(n => n.NetworkId == networkId && n.AccessId == accessdId);
        }

        public ICollection<NetworkAccess> GetByNetworkId(Guid id)
        {
            return [.. GetQueryable()
                .Where(np => np.NetworkId == id)];
        }

        public ICollection<NetworkAccess> GetByAccessId(Guid id)
        {
            return [.. GetQueryable()
                .Where(np => np.AccessId == id)];
        }

        public NetworkAccess Create(NetworkAccess networkAccess)
        {
            _context.NetworkAccesses.Add(networkAccess);
            SaveAndUpdate(networkAccess);
            return networkAccess;
        }

        public NetworkAccess Update(NetworkAccess networkAccess)
        {
            var existing = this.GetByIds(networkAccess.NetworkId, networkAccess.AccessId);
            if (existing == null) throw new Exception("NetworkAccess not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");

            _context.Entry(existing).CurrentValues.SetValues(networkAccess);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(NetworkAccess networkAccess)
        {
            if (networkAccess.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");
            
            _context.Remove(networkAccess);
            _context.SaveChanges();
        }
    }
}
