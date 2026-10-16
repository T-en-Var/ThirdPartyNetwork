using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkFeatureFlagRepository(DataContext context) : BaseRepository(context), INetworkFeatureFlagRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkFeatureFlag> GetQueryableAsync()
        {
            var query = _context.NetworkFeatureFlags
                .AsQueryable();

            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Networks.AnyAsync(n => n.Id == id);
        }

        public async Task<NetworkFeatureFlag?> GetByIdAsync(Guid networkId)
        {
            return await GetQueryableAsync().FirstOrDefaultAsync(x => x.NetworkId == networkId);
        }

        public async Task<NetworkFeatureFlag> UpdateAsync(NetworkFeatureFlag networkFeatureFlag)
        {
            var existing = await GetByIdAsync(networkFeatureFlag.NetworkId);
            if (existing == null) throw new Exception("NetworkFeatureFlag not found");

            _context.Entry(existing).CurrentValues.SetValues(networkFeatureFlag);

            await SaveAndUpdateAsync(existing);
            return existing;
        }
    }
}