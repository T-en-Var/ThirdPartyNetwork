using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;
using thirdpartynetwork.Models;
using thirdpartynetwork.Interfaces;

namespace thirdpartynetwork.Repositories
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
