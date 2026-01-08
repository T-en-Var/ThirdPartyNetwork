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

        private IQueryable<NetworkUser> GetQueryable(Guid networkId)
        {
            var query = _context.NetworkUsers
                // .AsNoTracking()  // Assume it doesnt break anything
                                    // because the issue arises as
                                    // UserProxy -> NetworkUsers is loaded into memory
                .AsQueryable();

            var currentUser = identityService.GetUserProxy(true);
            
            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadAccess))
                query = query
                    .Include(x => x.NetworkUserAccesses)
                    .ThenInclude(x => x.Access);

            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadUser))
                query = query
                    .Include(x => x.UserProxy)
                    .ThenInclude(x => x.ImageFile);

            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadRole))
                query = query
                    .Include(x => x.NetworkUserRoles)
                    .ThenInclude(x => x.Role);
            
            return query;
        }

        public bool Exists(Guid id)
        {
            return _context.NetworkUsers.Any(nu => nu.Id == id);
        }

        public ICollection<NetworkUser> GetAll()
        {
            return
            [
                .. _context.NetworkUsers
                    .Include(n => n.Network)
                    .Include(n => n.UserProxy)
                    .AsNoTracking()
            ];
        }

        public ICollection<NetworkUser> GetByNetworkId(Guid networkId)
        {
            return
            [
                .. GetQueryable(networkId)
                    .AsNoTracking()
                    .Where(nu => nu.NetworkId == networkId)
            ];
        }

        public ICollection<NetworkUser> GetByUserProxyId(
            Guid userProxyId,
            Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>? queryConfig = null)
        {
            // Start with the base query
            var query = _context.NetworkUsers.Include(x => x.Files).AsNoTracking().AsQueryable();

            // Apply the includes if provided
            if (queryConfig != null)
            {
                query = queryConfig(query);
            }

            // Apply the filter and execute
            return
            [
                .. query.Where(nu => nu.UserProxyId == userProxyId)
            ];
        }

        public NetworkUser? GetById(Guid networkId, Guid id)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(u => u.Id == id);
        }

        public NetworkUser? GetJustById(Guid id)
        {
            return _context.NetworkUsers.FirstOrDefault(u => u.Id == id);
        }

        public NetworkUser? GetByIds(Guid networkId, Guid userProxyId)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public NetworkUser? GetByIdsNoInclude(Guid networkId, Guid userProxyId)
        {
            return _context.NetworkUsers
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
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

        public NetworkUser? GetByIdsPreAuth(Guid networkId, Guid userProxyId)
        {
            return PreAuthIncludes(_context.NetworkUsers)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxyId == userProxyId);
        }

        public NetworkUser? GetByNetworkIdAndEmail(Guid networkId, string email)
        {
            // Used for login, so always requires UserProxy
            Console.WriteLine("Step 4, Getting queryable");
            var query = GetQueryable(networkId);
            Console.WriteLine("Step 4.2");
            return PreAuthIncludes(query)
                .FirstOrDefault(u => u.NetworkId == networkId && u.UserProxy.Email == email);
        }

        public NetworkUser Create(NetworkUser networkUser)
        {
            _context.NetworkUsers.Add(networkUser);
            SaveAndUpdate(networkUser);
            return networkUser;
        }

        public NetworkUser Update(Guid networkId, NetworkUser networkUser)
        {
            var existing = GetById(networkId, networkUser.Id);
            if (existing == null) throw new Exception("NetworkUser not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUser);

            SaveAndUpdate(existing);
            return existing;
        }

        public void NetworkAccessUpdate(Guid networkId)
        {
            _context.NetworkUsers
                .Where(u => u.NetworkId == networkId)
                .ExecuteUpdate(u => u.SetProperty(nu => nu.AccessIncomplete, true));
        }

        public void Delete(NetworkUser networkUser)
        {
            _context.Remove(networkUser);
            _context.SaveChanges();
        }
    }
}