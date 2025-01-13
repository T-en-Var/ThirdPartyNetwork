using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserProxyRepository(DataContext context) : BaseRepository(context), IUserProxyRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid userProxyId)
        {
            return _context.UserProxies.Any(up => up.Id == userProxyId);
        }

        public ICollection<UserProxy> GetAll()
        {
            return [.. _context.UserProxies];
        }

        public UserProxy? GetById(Guid userProxyId)
        {
            return _context.UserProxies
                .Include(up => up.User)
                .FirstOrDefault(up => up.Id == userProxyId);
        }

        public ICollection<UserProxy> GetByUserId(Guid userId)
        {
            return [.. _context.UserProxies
                .Where(up => up.User.Id == userId)
                .Include(up => up.User)];
        }

        public UserProxy Create(UserProxy userProxy)
        {
            _context.UserProxies.Add(userProxy);
            Save();
            return userProxy;
        }

        public UserProxy Update(UserProxy userProxy)
        {
            var existing = GetById(userProxy.Id);
            if (existing == null) throw new Exception("UserProxy not found");

            _context.Entry(existing).CurrentValues.SetValues(userProxy);

            Save();
            return existing;
        }

        public bool Delete(UserProxy userProxy)
        {
            _context.Remove(userProxy);
            return Save();
        }
    }
}
