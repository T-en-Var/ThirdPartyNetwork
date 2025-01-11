using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class PermissionRepository(DataContext context) : BaseRepository(context), IPermissionRepository
    {
        private readonly DataContext _context = context;

        public Permission Create(Permission permission)
        {
            _context.Permissions.Add(permission);
            Save();
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

        public Permission Update(Permission permission)
        {
            var existing = this.GetById(permission.Id);
            if (existing == null) throw new Exception("Permission not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Permission is system protected");

            _context.Entry(existing).CurrentValues.SetValues(permission);

            Save();
            return existing;
        }
    }
}
