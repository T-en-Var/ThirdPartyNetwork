﻿using Microsoft.EntityFrameworkCore;
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
        
        public async Task<bool> ExistsAsync(Guid networkUserId, Guid accessId)
        {
            return await _context.NetworkUserAccesses
                .AnyAsync(nup =>  nup.NetworkUserId == networkUserId && nup.AccessId == accessId);
        }

        public async Task<ICollection<NetworkUserAccess>> GetAllAsync()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task PopulateAsync(Guid networkId, Guid accessId)
        {
            var networkUsers = await _context.NetworkUsers
                .Where(u => u.NetworkId == networkId)
                .ToListAsync();

            var newAccesses = networkUsers.Select(nu => new NetworkUserAccess
            {
                NetworkUserId = nu.Id,
                AccessId = accessId,
                IsAccepted = false,
                NetworkUser = null,
                Access = null
            }).ToList();

            await _context.NetworkUserAccesses.AddRangeAsync(newAccesses);
            await SaveAndUpdateAllAsync(newAccesses);
        }

        public async Task DepopulateAsync(Guid networkId, Guid accessId)
        {
            var accessesToRemove = await _context.NetworkUserAccesses
                .Where(nua => nua.AccessId == accessId && nua.NetworkUser.NetworkId == networkId)
                .ToListAsync();

            _context.NetworkUserAccesses.RemoveRange(accessesToRemove);
            await _context.SaveChangesAsync();
        }

        public async Task<NetworkUserAccess?> GetByIdsAsync(Guid networkUserId, Guid accessId)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(n => n.NetworkUserId == networkUserId && n.AccessId == accessId);
        }

        public async Task<ICollection<NetworkUserAccess>> GetByNetworkUserIdAsync(Guid id)
        {
            return await GetQueryable()
                
                .Where(nup => nup.NetworkUserId == id).ToListAsync();
        }

        public async Task<ICollection<NetworkUserAccess>> GetByAccessIdAsync(Guid accessId)
        {
            return await GetQueryable()
                
                .Where(nup => nup.AccessId == accessId).ToListAsync();
        }

        public async Task<NetworkUserAccess> CreateAsync(NetworkUserAccess networkUserPermission)
        {
            await _context.NetworkUserAccesses.AddAsync(networkUserPermission);
            await SaveAndUpdateAsync(networkUserPermission);
            return networkUserPermission;
        }

        public async Task<NetworkUserAccess> UpdateAsync(NetworkUserAccess networkUserAccess)
        {
            var existing = await GetByIdsAsync(networkUserAccess.NetworkUserId, networkUserAccess.AccessId);
            if (existing == null) throw new Exception("NetworkUserPermission not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserAccess);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task MarkAccessCompleteAsync(Guid networkUserId)
        {
            // Fast check: are there any required accesses that are NOT accepted?
            bool hasUnacceptedRequiredAccess = await _context.NetworkAccesses
                .Where(na => na.IsRequired && na.Network.NetworkUsers.Any(nu => nu.Id == networkUserId))
                .AnyAsync(na => !_context.NetworkUserAccesses
                    .Any(nua => nua.NetworkUserId == networkUserId && nua.AccessId == na.AccessId && nua.IsAccepted)); // Inner Any is usually translated to SQL EXISTS, so it's fine inside AnyAsync

            if (hasUnacceptedRequiredAccess)
                return;

            var networkUser = await _context.NetworkUsers.FirstOrDefaultAsync(nu => nu.Id == networkUserId);
            if (networkUser == null)
                return;
            
            networkUser.AccessIncomplete = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(NetworkUserAccess networkUserAccess)
        {
            _context.Remove(networkUserAccess);
            await _context.SaveChangesAsync();
        }
    }
}
