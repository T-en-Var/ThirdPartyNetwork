﻿using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRoleRepository(DataContext context) : BaseRepository(context), INetworkUserRoleRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkUserRole> GetQueryable(Guid networkId)
        {
            var query = _context.NetworkUserRoles
                .AsQueryable();

            query = query
                .Include(x => x.Role)
                .Include(x => x.NetworkUser);

            return query;
        }

        public async Task<bool> ExistsAsync(Guid networkId, Guid networkUserId, Guid roleId)
        {
            return await _context.NetworkUserRoles
                .AnyAsync(nur => nur.NetworkUserId == networkUserId && nur.RoleId == roleId);
        }

        public async Task<ICollection<NetworkUserRole>> GetAllAsync()
        {
            return await _context.NetworkUserRoles
                    .Include(n => n.NetworkUser)
                    .Include(n => n.Role)
                    
                    .ToListAsync();
        }

        public async Task<NetworkUserRole?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId)
        {
            return await GetQueryable(networkId)
                .FirstOrDefaultAsync(n => n.NetworkUserId == networkUserId && n.RoleId == roleId);
        }

        public async Task<NetworkUserRole?> GetByIdAndNameAsync(Guid networkId, Guid networkUserId, string roleName)
        {
            return await GetQueryable(networkId)
                .FirstOrDefaultAsync(n => n.NetworkUserId == networkUserId && n.Role.Name == roleName);
        }

        public async Task<ICollection<NetworkUserRole>> GetByNetworkUserIdAsync(Guid networkId, Guid id)
        {
            return await GetQueryable(networkId)
                    
                    .Where(nur => nur.NetworkUserId == id)
                    .ToListAsync();
        }

        public async Task<ICollection<NetworkUserRole>> GetByRoleIdAsync(Guid networkId, Guid id)
        {
            return await GetQueryable(networkId)
                    
                    .Where(nur => nur.RoleId == id)
                    .ToListAsync();
        }

        public async Task<NetworkUserRole> CreateAsync(NetworkUserRole networkUserRole)
        {
            await _context.NetworkUserRoles.AddAsync(networkUserRole);
            await SaveAndUpdateAsync(networkUserRole);
            return networkUserRole;
        }

        public async Task<NetworkUserRole> UpdateAsync(Guid networkId, NetworkUserRole networkUserRole)
        {
            var existing = await GetByIdsAsync(networkId, networkUserRole.NetworkUserId, networkUserRole.RoleId);
            if (existing == null) throw new Exception("NetworkUserRole not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserRole);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(NetworkUserRole networkUserRole)
        {
            _context.Remove(networkUserRole);
            await _context.SaveChangesAsync();
        }
    }
}