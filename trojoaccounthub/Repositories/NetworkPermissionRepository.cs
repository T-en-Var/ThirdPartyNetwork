using Microsoft.EntityFrameworkCore;
using trojoaccounthub.Data;
using trojoaccounthub.Interfaces.Repositories;
using trojoaccounthub.Models;

namespace trojoaccounthub.Repositories
{
    public class NetworkPermissionRepository(DataContext _context) : BaseRepository(_context), INetworkPermissionRepository
    {
        public NetworkPermission? Create(NetworkPermission networkPermission)
        {
            _context.NetworkPermissions.Add(networkPermission);
            if (!Save()) return null;
            return networkPermission;
        }

        public bool Delete(NetworkPermission networkPermission)
        {
            _context.Remove(networkPermission);
            return Save();
        }

        public bool Exists(int networkId, int permissionId)
        {
            return _context.NetworkPermissions.Any(np => np.NetworkId == networkId && np.PermissionId == permissionId);
        }

        public ICollection<NetworkPermission> GetAll()
        {
            return [.. _context.NetworkPermissions];
        }

        public NetworkPermission? GetByIds(int networkId, int permissiondId)
        {
            return _context.NetworkPermissions.Find(networkId, permissiondId);
        }

        public ICollection<NetworkPermission> GetByNetworkId(int id)
        {
            return [.. _context.NetworkPermissions.Where(np => np.NetworkId == id)];
        }

        public ICollection<NetworkPermission> GetByPermissionId(int id)
        {
            return [.. _context.NetworkPermissions.Where(np => np.PermissionId == id)];
        }

        public NetworkPermission? Update(NetworkPermission networkPermission)
        {
            NetworkPermission? existing = this.GetByIds(networkPermission.NetworkId, networkPermission.PermissionId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(networkPermission);

            if (!Save()) return null;
            return existing;
        }
    }
}
