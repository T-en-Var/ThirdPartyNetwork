using trojoaccounthub.Data;
using trojoaccounthub.Interfaces.Repositories;
using trojoaccounthub.Models;

namespace trojoaccounthub.Repositories
{
    public class NetworkUserRoleRepository(DataContext _context) : BaseRepository(_context), INetworkUserRoleRepository
    {
        public NetworkUserRole? Create(NetworkUserRole networkUserRole)
        {
            _context.NetworkUserRoles.Add(networkUserRole);
            if (!Save()) return null;
            return networkUserRole;
        }

        public bool Delete(NetworkUserRole networkUserRole)
        {
            _context.Remove(networkUserRole);
            return Save();
        }

        public bool Exists(Guid networkUserId, Guid roleId)
        {
            return _context.NetworkUserRoles.Any(nur => nur.NetworkUserId == networkUserId && nur.RoleId == roleId);
        }

        public ICollection<NetworkUserRole> GetAll()
        {
            return [.. _context.NetworkUserRoles];
        }

        public NetworkUserRole? GetByIds(Guid networkUserId, Guid roleId)
        {
            return _context.NetworkUserRoles.Find(networkUserId, roleId);
        }

        public ICollection<NetworkUserRole> GetByNetworkUserId(Guid id)
        {
            return [.. _context.NetworkUserRoles.Where(nur => nur.NetworkUserId == id)];
        }

        public ICollection<NetworkUserRole> GetByRoleId(Guid id)
        {
            return [.. _context.NetworkUserRoles.Where(nur => nur.RoleId == id)];
        }

        public NetworkUserRole? Update(NetworkUserRole networkUserRole)
        {
            NetworkUserRole? existing = this.GetByIds(networkUserRole.NetworkUserId, networkUserRole.RoleId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(networkUserRole);

            if (!Save()) return null;
            return existing;
        }
    }
}
