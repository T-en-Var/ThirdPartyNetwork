using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRoleRepository(DataContext context) : BaseRepository(context), INetworkRoleRepository
    {
        private readonly DataContext _context = context;

        public NetworkRole Create(NetworkRole networkRole)
        {
            _context.NetworkRoles.Add(networkRole);
            Save();
            return networkRole;
        }

        public bool Delete(NetworkRole networkRole)
        {
            _context.Remove(networkRole);
            return Save();
        }

        public bool Exists(Guid networkId, Guid roleId)
        {
            return _context.NetworkRoles.Any(nr => nr.NetworkId == networkId && nr.RoleId == roleId);
        }

        public ICollection<NetworkRole> GetAll()
        {
            return [.. _context.NetworkRoles];
        }

        public NetworkRole? GetByIds(Guid networkId, Guid roleId)
        {
            return _context.NetworkRoles.Find(networkId, roleId);
        }

        public ICollection<NetworkRole> GetByNetworkId(Guid id)
        {
            return [.. _context.NetworkRoles.Where(nr => nr.NetworkId == id)];
        }

        public ICollection<NetworkRole> GetByRoleId(Guid id)
        {
            return [.. _context.NetworkRoles.Where(nr => nr.RoleId == id)];
        }

        public NetworkRole Update(NetworkRole networkRole)
        {
            var existing = this.GetByIds(networkRole.NetworkId, networkRole.RoleId);
            if (existing == null) throw new Exception("NetworkRole not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("NetworkRole is system protected");

            _context.Entry(existing).CurrentValues.SetValues(networkRole);

            Save();
            return existing;
        }
    }
}
