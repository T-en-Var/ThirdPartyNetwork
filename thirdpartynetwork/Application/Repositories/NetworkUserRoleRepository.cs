using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkUserRoleRepository(DataContext context) : BaseRepository(context), INetworkUserRoleRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<NetworkUserRole> GetQueryable(Guid networkId)
        {
            var query = _context.NetworkUserRoles
                .AsQueryable();

            query = query
                .Include(x => x.Role)
                .Include(x => x.NetworkUser);

            return query;
        }

        public bool Exists(Guid networkId, Guid networkUserId, Guid roleId)
        {
            return _context.NetworkUserRoles
                .Any(nur => nur.NetworkUserId == networkUserId && nur.RoleId == roleId);
        }

        public ICollection<NetworkUserRole> GetAll()
        {
            return
            [
                .. _context.NetworkUserRoles
                    .Include(n => n.NetworkUser)
                    .Include(n => n.Role)
                    .AsNoTracking()
            ];
        }

        public NetworkUserRole? GetByIds(Guid networkId, Guid networkUserId, Guid roleId)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.RoleId == roleId);
        }

        public NetworkUserRole? GetByIdAndName(Guid networkId, Guid networkUserId, string roleName)
        {
            return GetQueryable(networkId)
                .FirstOrDefault(n => n.NetworkUserId == networkUserId && n.Role.Name == roleName);
        }

        public ICollection<NetworkUserRole> GetByNetworkUserId(Guid networkId, Guid id)
        {
            return
            [
                .. GetQueryable(networkId)
                    .AsNoTracking()
                    .Where(nur => nur.NetworkUserId == id)
            ];
        }

        public ICollection<NetworkUserRole> GetByRoleId(Guid networkId, Guid id)
        {
            return
            [
                .. GetQueryable(networkId)
                    .AsNoTracking()
                    .Where(nur => nur.RoleId == id)
            ];
        }

        public NetworkUserRole Create(NetworkUserRole networkUserRole)
        {
            _context.NetworkUserRoles.Add(networkUserRole);
            SaveAndUpdate(networkUserRole);
            return networkUserRole;
        }

        public NetworkUserRole Update(Guid networkId, NetworkUserRole networkUserRole)
        {
            var existing = GetByIds(networkId, networkUserRole.NetworkUserId, networkUserRole.RoleId);
            if (existing == null) throw new Exception("NetworkUserRole not found");

            _context.Entry(existing).CurrentValues.SetValues(networkUserRole);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(NetworkUserRole networkUserRole)
        {
            _context.Remove(networkUserRole);
            _context.SaveChanges();
        }
    }
}