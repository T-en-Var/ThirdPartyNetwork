using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkEntitlementRepository(DataContext context) : BaseRepository(context), INetworkEntitlementRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkEntitlement> GetQueryableAsync()
        {
            var query = _context.NetworkEntitlements
                .AsQueryable();

            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Networks.AnyAsync(n => n.Id == id);
        }

        public async Task<NetworkEntitlement?> GetByIdAsync(Guid networkId)
        {
            return await GetQueryableAsync().FirstOrDefaultAsync(x => x.NetworkId == networkId);
        }

        public async Task<NetworkEntitlement> UpdateAsync(NetworkEntitlement networkEntitlement)
        {
            var existing = await GetByIdAsync(networkEntitlement.NetworkId);
            if (existing == null) throw new Exception("NetworkEntitlement not found");

            _context.Entry(existing).CurrentValues.SetValues(networkEntitlement);

            await SaveAndUpdateAsync(existing);
            return existing;
        }
    }
}