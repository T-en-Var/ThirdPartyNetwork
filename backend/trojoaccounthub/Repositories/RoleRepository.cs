using System.Security;
using trojoaccounthub.Data;
using trojoaccounthub.Interfaces.Repositories;
using trojoaccounthub.Models;

namespace trojoaccounthub.Repositories
{
    public class RoleRepository(DataContext _context) : BaseRepository(_context), IRoleRepository
    {
        public Role? Create(Role role)
        {
            _context.Roles.Add(role);
            if (!Save()) return null;
            return role;
        }

        public bool Delete(Role role)
        {
            _context.Remove(role);
            return Save();
        }

        public bool Exists(int id)
        {
            return _context.Roles.Any(r => r.Id == id);
        }

        public ICollection<Role> GetAll()
        {
            return [.. _context.Roles];
        }

        public Role? GetById(int id)
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
