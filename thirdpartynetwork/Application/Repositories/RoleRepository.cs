﻿using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class RoleRepository(DataContext context, IIdentityVerifier identityVerifier, IIdentityService identityService) : BaseRepository(context), IRoleRepository
    {
        private readonly DataContext _context = context;

        private async Task<IQueryable<Role>> GetQueryableAsync(Guid networkId)
        {
            var query = _context.Roles
                .Where(r => r.NetworkId == networkId)
                .AsQueryable();
            
            var currentUser = await identityService.GetUserProxyAsync(true);
            
            if (await identityVerifier.IsAllowedAsync(networkId, currentUser.Id, Constants.Permissions.ReadPermission))
                query = query
                    .Include(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission);
            
            if (await identityVerifier.IsAllowedAsync(networkId, currentUser.Id, Constants.Permissions.ReadUser))
                query = query
                    .Include(x => x.NetworkUserRoles)
                    .ThenInclude(x => x.NetworkUser)
                    .ThenInclude(x => x.UserProxy)
                    .ThenInclude(x => x.ImageFile);
            
            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Roles.AnyAsync(r => r.Id == id);
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        public async Task<ICollection<Role>> GetByNetworkIdAsync(Guid networkId)
        {
            return await (await GetQueryableAsync(networkId)).AsNoTracking().ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(Guid networkId, Guid id)
        {
            return await (await GetQueryableAsync(networkId)).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await SaveAndUpdateAsync(role);
            return role;
        }

        public async Task<Role> UpdateAsync(Guid networkId, Role role)
        {
            var existing = await GetByIdAsync(networkId, role.Id);
            if (existing == null) throw new Exception("Role not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Entry(existing).CurrentValues.SetValues(role);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(Role role)
        {
            if (role.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}
