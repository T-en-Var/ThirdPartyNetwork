using System.Security;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class RoleRepository(DataContext context) : BaseRepository(context), IRoleRepository
    {
        private readonly DataContext _context = context;

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
            return [.. _context.Roles
                .Where(r => r.NetworkId == networkId)];
        }

        public Role? GetById(Guid id)
        {
            return _context.Roles.Find(id);
        }

        public Role Create(Role role)
        {
            _context.Roles.Add(role);
            Save();
            return role;
        }

        public Role Update(Role role)
        {
            var existing = this.GetById(role.Id);
            if (existing == null) throw new Exception("Role not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Entry(existing).CurrentValues.SetValues(role);

            Save();
            return existing;
        }

        public bool Delete(Role role)
        {
            if (role.IsSystemProtected) throw new UnauthorizedHttpException("Role is system protected");

            _context.Remove(role);
            return Save();
        }
    }
}
