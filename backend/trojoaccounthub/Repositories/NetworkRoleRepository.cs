using Microsoft.EntityFrameworkCore;
using trojoaccounthub.Data;
using trojoaccounthub.Interfaces;
using trojoaccounthub.Models;

namespace trojoaccounthub.Repositories
{
    public class NetworkRoleRepository(DataContext _context) : BaseRepository(_context), INetworkRoleRepository
    {
        public NetworkRole? Create(NetworkRole networkRole)
        {
            _context.NetworkRoles.Add(networkRole);
            if (!Save()) return null;
            return networkRole;
        }

        public bool Delete(NetworkRole networkRole)
        {
            _context.Remove(networkRole);
            return Save();
        }

        public bool Exists(int networkId, int roleId)
        {
            return _context.NetworkRoles.Any(nr => nr.NetworkId == networkId && nr.RoleId == roleId);
        }

        public ICollection<NetworkRole> GetAll()
        {
            return [.. _context.NetworkRoles];
        }

        public NetworkRole? GetByIds(int networkId, int roleId)
        {
            return _context.NetworkRoles.Find(networkId, roleId);
        }

        public ICollection<NetworkRole> GetByNetworkId(int id)
        {
            return [.. _context.NetworkRoles.Where(nr => nr.NetworkId == id)];
        }

        public ICollection<NetworkRole> GetByRoleId(int id)
        {
            return [.. _context.NetworkRoles.Where(nr => nr.RoleId == id)];
        }

        public NetworkRole? Update(NetworkRole networkRole)
        {
            NetworkRole? existing = this.GetByIds(networkRole.NetworkId, networkRole.RoleId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(networkRole);

            if (!Save()) return null;
            return existing;
        }
    }
}
