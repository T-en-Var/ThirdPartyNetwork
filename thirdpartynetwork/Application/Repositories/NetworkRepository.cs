using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(DataContext context, IIdentityService identityService, IIdentityVerifier identityVerifier) : BaseRepository(context), INetworkRepository
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

        private IQueryable<Network> GetQueryableReduced(Guid id)
        {
            var query = _context.Networks.AsQueryable();
            var currentUser = identityService.GetUserProxy(true);

            var allowUsers = identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadUser);
            var allowAccess = identityVerifier.IsAllowed(id, currentUser.Id, Constants.Permissions.ReadAccess);
            
            if (allowUsers)
                query = query
                    .Include(x => x.NetworkUsers)
                    .ThenInclude(x => x.UserProxy);
            
            if (allowAccess)
                query = query
                    .Include(x => x.NetworkAccesses)
                    .ThenInclude(x => x.Access);
            
            return query;
        }

        public bool Exists(Guid id)
        {
            return _context.Networks.Any(n => n.Id == id);
        }

        public ICollection<Network> GetAll()
        {
            var mainNetwork = GetMainNetwork();
            return [.. GetQueryableReduced(mainNetwork.Id)];
        }

        public Network? GetById(Guid id, bool bypass = false)
        {
            return GetQueryable(id, bypass).FirstOrDefault(x => x.Id == id);
        }

        public Network? GetDetailsById(Guid id)
        {
            var network = _context.Networks
                .Include(x => x.NetworkAccesses)
                .ThenInclude(x => x.Access)
                .Include(x => x.NetworkUsers)
                .ThenInclude(n => n.NetworkUserAccesses)
                .Include(n => n.ImageFile)
                .AsNoTracking()
                .FirstOrDefault(n => n.Id == id);
            return network;
        }

        public Network GetMainNetwork()
        {
            var mainNetwork = _context.Networks.FirstOrDefault(n => n.Name == Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");
            
            return mainNetwork;
        }

        public Network? GetByName(string name)
        {
            var mainNetwork = GetMainNetwork();
            return GetQueryable(mainNetwork.Id).FirstOrDefault(n => n.Name == name);
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

        public Network Create(Network network)
        {
            _context.Networks.Add(network);
            SaveAndUpdate(network);
            return network;
        }

        public Network Update(Network network)
        {
            var existing = GetById(network.Id);
            if (existing == null) throw new Exception("Network not found");

            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");

            _context.Entry(existing).CurrentValues.SetValues(network);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(Network network)
        {
            if (network.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");
            
            _context.Remove(network);
            _context.SaveChanges();
        }
    }
}
