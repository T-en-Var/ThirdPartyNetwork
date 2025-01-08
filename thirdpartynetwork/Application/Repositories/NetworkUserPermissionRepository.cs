using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Repositories
{
    public class NetworkUserPermissionRepository(DataContext _context) : BaseRepository(_context), INetworkUserPermissionRepository
    {
        public NetworkUserPermission? Create(NetworkUserPermission networkUserPermission)
        {
            _context.NetworkUserPermissions.Add(networkUserPermission);
            if (!Save()) return null;
            return networkUserPermission;
        }

        public bool Delete(NetworkUserPermission networkUserPermission)
        {
            _context.Remove(networkUserPermission);
            return Save();
        }

        public bool Exists(Guid networkUserId, Guid permissionId)
        {
            return _context.NetworkUserPermissions.Any(nup =>  nup.NetworkUserId == networkUserId && nup.PermissionId == permissionId);
        }

        public ICollection<NetworkUserPermission> GetAll()
        {
            return [.. _context.NetworkUserPermissions];
        }

        public NetworkUserPermission? GetByIds(Guid networkUserId, Guid permissionId)
        {
            return _context.NetworkUserPermissions.Find(networkUserId, permissionId);
        }

        public ICollection<NetworkUserPermission> GetByNetworkUserId(Guid id)
        {
            return [.. _context.NetworkUserPermissions.Where(nup => nup.NetworkUserId == id)];
        }

        public ICollection<NetworkUserPermission> GetByPermissionId(Guid id)
        {
            return [.. _context.NetworkUserPermissions.Where(nup => nup.PermissionId == id)];
        }

        public NetworkUserPermission? Update(NetworkUserPermission networkUserPermission)
        {
            NetworkUserPermission? existing = this.GetByIds(networkUserPermission.NetworkUserId, networkUserPermission.PermissionId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(networkUserPermission);

            if (!Save()) return null;
            return existing;
        }
    }
}
