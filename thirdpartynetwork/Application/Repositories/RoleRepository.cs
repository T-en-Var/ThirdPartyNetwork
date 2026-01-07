using Microsoft.EntityFrameworkCore;
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

        private IQueryable<Role> GetQueryable(Guid networkId)
        {
            var query = _context.Roles
                .Where(r => r.NetworkId == networkId)
                .AsQueryable();
            var currentUser = identityService.GetUserProxy(true);
            
            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadPermission))
                query = query
                    .Include(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission);
            
            if (identityVerifier.IsAllowed(networkId, currentUser.Id, Constants.Permissions.ReadUser))
                query = query
                    .Include(x => x.NetworkUserRoles)
                    .ThenInclude(x => x.NetworkUser)
                    .ThenInclude(x => x.UserProxy)
                    .ThenInclude(x => x.ImageFile);
            
            return query;
        }

        public bool Exists(Guid id)
        {
            return _context.Roles.Any(r => r.Id == id);
        }

        public ICollection<Role> GetAll()
        {
            return [.. _context.Roles];
        }

        public ICollection<Role> GetByNetworkId(Guid networkId)
        {
            return [.. GetQueryable(networkId)];
        }

        public Role? GetById(Guid networkId, Guid id)
        {
            return GetQueryable(networkId).FirstOrDefault(r => r.Id == id);
        }

        public Role Create(Role role)
        {
            _context.Roles.Add(role);
            SaveAndUpdate(role);
            return role;
        }

        public Role Update(Guid networkId, Role role)
        {
            var existing = GetById(networkId, role.Id);
            if (existing == null) throw new Exception("Role not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Entry(existing).CurrentValues.SetValues(role);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(Role role)
        {
            if (role.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Remove(role);
            _context.SaveChanges();
        }
    }
}
