using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkPermissionRepository(DataContext context) : BaseRepository(context), INetworkPermissionRepository
    {
        private readonly DataContext _context = context;

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

        public NetworkPermission Create(NetworkPermission networkPermission)
        {
            _context.NetworkPermissions.Add(networkPermission);
            Save();
            return networkPermission;
        }

        public NetworkPermission Update(NetworkPermission networkPermission)
        {
            var existing = this.GetByIds(networkPermission.NetworkId, networkPermission.PermissionId);
            if (existing == null) throw new Exception("NetworkPermission not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("NetworkPermission is system protected");

            _context.Entry(existing).CurrentValues.SetValues(networkPermission);

            Save();
            return existing;
        }

        public bool Delete(NetworkPermission networkPermission)
        {
            if (networkPermission.IsSystemProtected) throw new UnauthorizedHttpException("NetworkPermission is system protected");
            
            _context.Remove(networkPermission);
            return Save();
        }
    }
}
