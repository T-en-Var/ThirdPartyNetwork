using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRepository(
        DataContext context,
        IIdentityService identityService,
        IIdentityVerifier identityVerifier) : BaseRepository(context), INetworkUserRepository
    {
        private readonly DataContext _context = context;

        private async Task<IQueryable<NetworkUser>> GetQueryableAsync(Guid networkId)
        {
            var query = _context.NetworkUsers
                .AsQueryable();

            var currentUser = await identityService.GetUserProxyAsync(true);
            
            if (await identityVerifier.IsAllowedAsync(networkId, currentUser.Id, Constants.Permissions.ReadAccess))
                query = query
                    .Include(x => x.NetworkUserAccesses)
                    .ThenInclude(x => x.Access);

            if (await identityVerifier.IsAllowedAsync(networkId, currentUser.Id, Constants.Permissions.ReadUser))
                query = query
                    .Include(x => x.UserProxy)
                    .ThenInclude(x => x.ImageFile);

            if (await identityVerifier.IsAllowedAsync(networkId, currentUser.Id, Constants.Permissions.ReadRole))
                query = query
                    .Include(x => x.NetworkUserRoles)
                    .ThenInclude(x => x.Role);
            
            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.NetworkUsers.AnyAsync(nu => nu.Id == id);
        }

        public async Task<ICollection<NetworkUser>> GetAllAsync()
        {
            return await _context.NetworkUsers
                    .Include(n => n.Network)
                    .Include(n => n.UserProxy)
                    
                    .ToListAsync();
        }

        public async Task<ICollection<NetworkUser>> GetByNetworkIdAsync(Guid networkId)
        {
            return await (await GetQueryableAsync(networkId))
                    
                    .Where(nu => nu.NetworkId == networkId)
                    .ToListAsync();
        }

        public async Task<ICollection<NetworkUser>> GetByUserProxyIdAsync(
            Guid userProxyId,
            Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>? queryConfig = null)
        {
            var query = _context.NetworkUsers.AsQueryable();
            
            if (queryConfig != null)
            {
                query = queryConfig(query);
            }

            return await query.Where(nu => nu.UserProxyId == userProxyId).ToListAsync();
        }

        public async Task<NetworkUser?> GetByIdAsync(Guid networkId, Guid id)
        {
            return await (await GetQueryableAsync(networkId))
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<NetworkUser?> GetJustByIdAsync(Guid id)
        {
            return await _context.NetworkUsers.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<NetworkUser?> GetByIdsAsync(Guid networkId, Guid userProxyId)
        {
            return await (await GetQueryableAsync(networkId))
                .FirstOrDefaultAsync(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public async Task<NetworkUser?> GetByIdsNoIncludeAsync(Guid networkId, Guid userProxyId)
        {
            return await _context.NetworkUsers
                .FirstOrDefaultAsync(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        private IQueryable<NetworkUser> PreAuthIncludes(IQueryable<NetworkUser> query)
        {
            return query
                .Include(n => n.UserProxy)
                .ThenInclude(n => n.NetworkUsers)
                .ThenInclude(n => n.NetworkUserRoles)
                .ThenInclude(nr => nr.Role)
                .ThenInclude(nr => nr.RolePermissions)
                .ThenInclude(nr => nr.Permission)
                .AsQueryable();
        }

        public async Task<NetworkUser?> GetByIdsPreAuthAsync(Guid networkId, Guid userProxyId)
        {
            return await PreAuthIncludes(_context.NetworkUsers)
                .FirstOrDefaultAsync(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public async Task<NetworkUser?> GetByNetworkIdAndEmailAsync(Guid networkId, string email)
        {
            // Used for login, so always requires UserProxy
            var query = await GetQueryableAsync(networkId);
            return await PreAuthIncludes(query)
                .FirstOrDefaultAsync(u => u.NetworkId == networkId && u.UserProxy.Email == email);
        }

        public async Task<NetworkUser> CreateAsync(NetworkUser networkUser)
        {
            await _context.NetworkUsers.AddAsync(networkUser);
            await SaveAndUpdateAsync(networkUser);
            return networkUser;
        }

        public async Task<NetworkUser> UpdateAsync(Guid networkId, NetworkUser networkUser)
        {
            var existing = await GetByIdAsync(networkId, networkUser.Id);
            if (existing == null) throw new Exception("NetworkUser not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUser);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task NetworkAccessUpdateAsync(Guid networkId)
        {
            await _context.NetworkUsers
                .Where(u => u.NetworkId == networkId)
                .ExecuteUpdateAsync(u => u.SetProperty(nu => nu.AccessIncomplete, true));
        }

        public async Task DeleteAsync(NetworkUser networkUser)
        {
            _context.Remove(networkUser);
            await _context.SaveChangesAsync();
        }
    }
}