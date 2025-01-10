using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkPermissionRepository(DataContext context) : BaseRepository(context), INetworkPermissionRepository
    {
        private readonly DataContext _context = context;

        public NetworkPermission? Create(NetworkPermission networkPermission)
        {
            _context.NetworkPermissions.Add(networkPermission);
            return !Save() ? null : networkPermission;
        }

        public bool Delete(NetworkPermission networkPermission)
        {
            _context.Remove(networkPermission);
            return Save();
        }

        public bool Exists(Guid networkId, Guid permissionId)
        {
            return _context.NetworkPermissions.Any(np => np.NetworkId == networkId && np.PermissionId == permissionId);
        }

        public ICollection<NetworkPermission> GetAll()
        {
            return [.. _context.NetworkPermissions];
        }

        public NetworkPermission? GetByIds(Guid networkId, Guid permissiondId)
        {
            return _context.NetworkPermissions.Find(networkId, permissiondId);
        }

        public ICollection<NetworkPermission> GetByNetworkId(Guid id)
        {
            return [.. _context.NetworkPermissions.Where(np => np.NetworkId == id)];
        }

        public ICollection<NetworkPermission> GetByPermissionId(Guid id)
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
