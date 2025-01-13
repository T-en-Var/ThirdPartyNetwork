using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserPermissionRepository(DataContext context) : BaseRepository(context), INetworkUserPermissionRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid networkUserId, Guid permissionId)
        {
            return _context.NetworkUserPermissions.Any(nup =>  nup.NetworkUserId == networkUserId && nup.PermissionId == permissionId);
        }

        public ICollection<NetworkUserPermission> GetAll()
        {
            return [.. _context.NetworkUserPermissions
                .Include(n => n.NetworkUser)
                .Include(n => n.Permission)];
        }

        public NetworkUserPermission? GetByIds(Guid networkUserId, Guid permissionId)
        {
            return _context.NetworkUserPermissions
                .Include(n => n.NetworkUser)
                .Include(n => n.Permission)
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.PermissionId == permissionId);
        }

        public ICollection<NetworkUserPermission> GetByNetworkUserId(Guid id)
        {
            return [.. _context.NetworkUserPermissions
                .Where(nup => nup.NetworkUserId == id)
                .Include(n => n.NetworkUser)
                .Include(n => n.Permission)];
        }

        public ICollection<NetworkUserPermission> GetByPermissionId(Guid id)
        {
            return [.. _context.NetworkUserPermissions
                .Where(nup => nup.PermissionId == id)
                .Include(n => n.NetworkUser)
                .Include(n => n.Permission)];
        }

        public NetworkUserPermission Create(NetworkUserPermission networkUserPermission)
        {
            _context.NetworkUserPermissions.Add(networkUserPermission);
            Save();
            return networkUserPermission;
        }

        public NetworkUserPermission Update(NetworkUserPermission networkUserPermission)
        {
            var existing = GetByIds(networkUserPermission.NetworkUserId, networkUserPermission.PermissionId);
            if (existing == null) throw new Exception("NetworkUserPermission not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserPermission);

            Save();
            return existing;
        }

        public bool Delete(NetworkUserPermission networkUserPermission)
        {
            _context.Remove(networkUserPermission);
            return Save();
        }
    }
}
