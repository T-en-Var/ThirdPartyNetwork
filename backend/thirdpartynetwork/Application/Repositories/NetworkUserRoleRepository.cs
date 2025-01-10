using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRoleRepository(DataContext context) : BaseRepository(context), INetworkUserRoleRepository
    {
        private readonly DataContext _context = context;

        public NetworkUserRole? Create(NetworkUserRole networkUserRole)
        {
            _context.NetworkUserRoles.Add(networkUserRole);
            return !Save() ? null : networkUserRole;
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
