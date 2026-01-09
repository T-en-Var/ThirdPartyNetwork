﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> ExistsAsync(Guid networkId, Guid accessId)
        {
            return await _context.NetworkAccesses.AnyAsync(np => np.NetworkId == networkId && np.AccessId == accessId);
        }

        public async Task<ICollection<NetworkAccess>> GetAllAsync()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task<NetworkAccess?> GetByIdsAsync(Guid networkId, Guid accessdId)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(n => n.NetworkId == networkId && n.AccessId == accessdId);
        }

        public async Task<ICollection<NetworkAccess>> GetByNetworkIdAsync(Guid id)
        {
            return await GetQueryable()
                .Where(np => np.NetworkId == id).ToListAsync();
        }

        public async Task<ICollection<NetworkAccess>> GetByAccessIdAsync(Guid id)
        {
            return await GetQueryable()
                .Where(np => np.AccessId == id).ToListAsync();
        }

        public async Task<NetworkAccess> CreateAsync(NetworkAccess networkAccess)
        {
            await _context.NetworkAccesses.AddAsync(networkAccess);
            await SaveAndUpdateAsync(networkAccess);
            return networkAccess;
        }

        public async Task<NetworkAccess> UpdateAsync(NetworkAccess networkAccess)
        {
            var existing = await this.GetByIdsAsync(networkAccess.NetworkId, networkAccess.AccessId);
            if (existing == null) throw new Exception("NetworkAccess not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");

            _context.Entry(existing).CurrentValues.SetValues(networkAccess);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(NetworkAccess networkAccess)
        {
            if (networkAccess.IsSystemProtected) throw new UnauthorizedHttpException("NetworkAccess is system protected");
            
            _context.Remove(networkAccess);
            await _context.SaveChangesAsync();
        }
    }
}
