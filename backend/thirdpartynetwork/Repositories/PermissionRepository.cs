using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Repositories
{
    public class PermissionRepository(DataContext _context) : BaseRepository(_context), IPermissionRepository
    {
        public Permission? Create(Permission permission)
        {
            _context.Permissions.Add(permission);
            if (!Save()) return null;
            return permission;
        }

        public bool Delete(Permission permission)
        {
            _context.Remove(permission);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.Permissions.Any(p => p.Id == id);
        }

        public ICollection<Permission> GetAll()
        {
            return [.. _context.Permissions];
        }

        public Permission? GetById(Guid id)
        {
            return _context.Permissions.Find(id);
        }

        public Permission? Update(Permission permission)
        {
            Permission? existing = this.GetById(permission.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(permission);

            if (!Save()) return null;
            return existing;
        }
    }
}
