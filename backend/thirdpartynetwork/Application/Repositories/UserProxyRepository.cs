﻿using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserProxyRepository(
        DataContext context,
        IIdentityVerifier identityVerifier,
        IIdentityService identityService) : BaseRepository(context), IUserProxyRepository
    {
        private readonly DataContext _context = context;

        private Task<IQueryable<UserProxy>> GetQueryableAsync()
        {
            var query = _context.UserProxies.AsQueryable();

            query = query
                // .AsNoTracking()  // Assume it doesnt break anything
                                    // because the issue arises as
                                    // UserProxy -> NetworkUsers is loaded into memory
                .Include(x => x.User)
                .ThenInclude(x => x.UserProxies)
                .Include(x => x.NetworkUsers)
                .ThenInclude(x => x.Network);

            return Task.FromResult(query);
        }

        public async Task<bool> ExistsAsync(Guid userProxyId)
        {
            return await _context.UserProxies.AnyAsync(up => up.Id == userProxyId);
        }

        public async Task<ICollection<UserProxy>> GetAllAsync()
        {
            return await (await GetQueryableAsync()).AsNoTracking().ToListAsync();
        }

        public async Task<UserProxy?> GetByIdAsync(Guid userProxyId)
        {
            var query = await GetQueryableAsync();
            var currentUser = await identityService.GetUserProxyAsync(true);

            if (await identityVerifier.IsUserProxyOwnerAsync(currentUser.Id, userProxyId))
            {
                query = query
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.NetworkUsers)
                    // .ThenInclude(nu => nu.Files)
                    
                    .Include(up => up.NetworkUsers)
                    // .ThenInclude(nu => nu.Files)
                    
                    .Include(up => up.ImageFile)
                    
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Network)
                    .ThenInclude(nu => nu.ImageFile)
                    
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.ImageFile)
                    
                    .Include(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.NetworkUserAccesses)
                    .ThenInclude(nua => nua.Access)
                    
                    .Include(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Network)
                    .ThenInclude(nu => nu.ImageFile);
            }

            return await query.FirstOrDefaultAsync(up => up.Id == userProxyId);
        }

        public async Task<UserProxy?> GetByUsernameAsync(string username)
        {
            var query = await GetQueryableAsync();

            return await query.FirstOrDefaultAsync(up => up.Username == username);
        }

        public async Task<ICollection<UserProxy>> GetByUserIdAsync(Guid userId)
        {
            return await (await GetQueryableAsync())
                    .AsNoTracking()
                    .Where(up => up.User.Id == userId)
                    .ToListAsync();
        }

        public async Task<UserProxy> CreateAsync(UserProxy userProxy)
        {
            await _context.UserProxies.AddAsync(userProxy);
            await SaveAndUpdateAsync(userProxy);
            return userProxy;
        }

        public async Task SetDefaultUserProxyAsync(Guid userProxyId)
        {
            var userProxy = await GetByIdAsync(userProxyId);
            if (userProxy == null) throw new Exception("UserProxy not found");
            
            if (userProxy.IsDefault) return;
            
            foreach (var userUserProxy in userProxy.User.UserProxies)
            {
                userUserProxy.IsDefault = false;
            }
            
            userProxy.IsDefault = true;
            // don't save, assume this is part of another process
            // assume that process calls _context.SaveChanges();
        }

        public async Task<UserProxy> UpdateAsync(UserProxy userProxy)
        {
            var existing = await GetByIdAsync(userProxy.Id);
            if (existing == null) throw new Exception("UserProxy not found");

            _context.Entry(existing).CurrentValues.SetValues(userProxy);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task<IQueryable<UserProxy>> GetByIdQueryableAsync(Guid id)
        {
            return (await GetQueryableAsync())
                .Where(u => u.Id == id);
        }

        public async Task DeleteAsync(UserProxy userProxy)
        {
            _context.Remove(userProxy);
            await _context.SaveChangesAsync();
        }
    }
}