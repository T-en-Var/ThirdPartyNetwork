using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRoleRepository(DataContext context) : BaseRepository(context), INetworkUserRoleRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid networkUserId, Guid roleId)
        {
            return _context.NetworkUserRoles.Any(nur => nur.NetworkUserId == networkUserId && nur.RoleId == roleId);
        }

        public ICollection<NetworkUserRole> GetAll()
        {
            return [.. _context.NetworkUserRoles
                .Include(n => n.NetworkUser)
                .Include(n => n.Role)];
        }

        public NetworkUserRole? GetByIds(Guid networkUserId, Guid roleId)
        {
            return _context.NetworkUserRoles
                .Include(n => n.NetworkUser)
                .Include(n => n.Role)
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.RoleId == roleId);
        }

        public ICollection<NetworkUserRole> GetByNetworkUserId(Guid id)
        {
            return [.. _context.NetworkUserRoles
                .Where(nur => nur.NetworkUserId == id)
                .Include(n => n.NetworkUser)
                .Include(n => n.Role)];
        }

        public ICollection<NetworkUserRole> GetByRoleId(Guid id)
        {
            return [.. _context.NetworkUserRoles
                .Where(nur => nur.RoleId == id)
                .Include(n => n.NetworkUser)
                .Include(n => n.Role)];
        }

        public NetworkUserRole Create(NetworkUserRole networkUserRole)
        {
            _context.NetworkUserRoles.Add(networkUserRole);
            Save();
            return networkUserRole;
        }

        public NetworkUserRole Update(NetworkUserRole networkUserRole)
        {
            var existing = this.GetByIds(networkUserRole.NetworkUserId, networkUserRole.RoleId);
            if (existing == null) throw new Exception("NetworkUserRole not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserRole);

            Save();
            return existing;
        }

        public bool Delete(NetworkUserRole networkUserRole)
        {
            _context.Remove(networkUserRole);
            return Save();
        }
    }
}
