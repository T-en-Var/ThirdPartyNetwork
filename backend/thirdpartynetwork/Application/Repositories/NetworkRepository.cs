﻿using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(DataContext context) : BaseRepository(context), INetworkRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<Network> GetQueryable(Guid id, bool bypass = false)
        {
            var query = _context.Networks
                .Include(x => x.ImageFile)
                .Include(x => x.NetworkUsers)
                .Include(x => x.Roles)
                .Include(x => x.NetworkAccesses)
                .ThenInclude(x => x.Access)
                .AsQueryable();
            // var currentUser = identityService.GetUserProxy(true);

            // var allowUsers = bypass || identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadUser);
            // var allowRoles = bypass || identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadRole);
            // var allowAccess = bypass || identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadAccess);
            // var allowPerms = bypass || identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadPermission);
            
            // if (allowUsers)
            // {
            //     query = query
            //         .Include(x => x.NetworkUsers)
            //         .ThenInclude(x => x.UserProxy);
            //
            //     if (allowRoles)
            //         query = query
            //             .Include(x => x.NetworkUsers)
            //             .ThenInclude(x => x.NetworkUserRoles)
            //             .ThenInclude(x => x.Role);
            //
            //     if (allowAccess)
            //         query = query
            //             .Include(x => x.NetworkUsers)
            //             .ThenInclude(x => x.NetworkUserAccesses)
            //             .ThenInclude(x => x.Access);
            // }
            //
            // if (allowRoles)
            // {
            //     query = query
            //         .Include(x => x.Roles);
            //     
            //     if (allowPerms)
            //         query = query
            //             .Include(x => x.Roles)
            //             .ThenInclude(x => x.RolePermissions)
            //             .ThenInclude(x => x.Permission);
            // }
            //
            // if (allowAccess)
            //     query = query
            //         .Include(x => x.NetworkAccesses)
            //         .ThenInclude(x => x.Access);
            
            return query;
        }

        private IQueryable<Network> GetQueryableReduced()
        {
            var query = _context.Networks
                .Include(x => x.NetworkUsers)
                .AsQueryable();
            
            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Networks.AnyAsync(n => n.Id == id);
        }

        public async Task<ICollection<Network>> GetAllAsync()
        {
            return await GetQueryableReduced().ToListAsync();
        }

        public async Task<Network?> GetByIdAsync(Guid id, bool bypass = false)
        {
            return await GetQueryable(id, bypass).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Network?> GetDetailsByIdAsync(Guid id)
        {
            var network = await _context.Networks
                .Include(x => x.NetworkAccesses)
                .ThenInclude(x => x.Access)
                .Include(x => x.NetworkUsers)
                // .ThenInclude(n => n.NetworkUserAccesses)
                .Include(n => n.ImageFile)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);
            return network;
        }

        public async Task<Network> GetMainNetworkAsync()
        {
            var mainNetwork = await _context.Networks.FirstOrDefaultAsync(n => n.Name == Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");
            
            return mainNetwork;
        }

        public async Task<Network?> GetByNameAsync(string name)
        {
            var mainNetwork = await GetMainNetworkAsync();
            return await GetQueryable(mainNetwork.Id).FirstOrDefaultAsync(n => n.Name == name);
        }

        public IQueryable<Network> GetByNameQueryable(string name)
        {
            return _context.Networks
                .Where(n => n.Name == name);
        }

        public IQueryable<Network> GetByIdQueryable(Guid id)
        {
            return _context.Networks
                .Where(n => n.Id == id);
        }

        public async Task<Network> CreateAsync(Network network)
        {
            await _context.Networks.AddAsync(network);
            await SaveAndUpdateAsync(network);
            return network;
        }

        public async Task<Network> UpdateAsync(Network network)
        {
            var existing = await GetByIdAsync(network.Id);
            if (existing == null) throw new Exception("Network not found");

            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");

            _context.Entry(existing).CurrentValues.SetValues(network);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(Network network)
        {
            if (network.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");
            
            _context.Remove(network);
            await _context.SaveChangesAsync();
        }
    }
}
