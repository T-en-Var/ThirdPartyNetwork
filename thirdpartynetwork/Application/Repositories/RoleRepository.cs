using System.Security;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class RoleRepository(DataContext context) : BaseRepository(context), IRoleRepository
    {
        private readonly DataContext _context = context;

        public Role? Create(Role role)
        {
            _context.Roles.Add(role);
            return !Save() ? null : role;
        }

        public bool Delete(Role role)
        {
            _context.Remove(role);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.Roles.Any(r => r.Id == id);
        }

        public ICollection<Role> GetAll()
        {
            return [.. _context.Roles];
        }

        public Role? GetById(Guid id)
        {
            return _context.Roles.Find(id);
        }

        public Role? Update(Role role)
        {
            Role? existing = this.GetById(role.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(role);

            if (!Save()) return null;
            return existing;
        }
    }
}
