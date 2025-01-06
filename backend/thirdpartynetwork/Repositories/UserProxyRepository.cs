using Microsoft.EntityFrameworkCore;
using System.Data;
using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Repositories
{
    public class UserProxyRepository(DataContext _context) : BaseRepository(_context), IUserProxyRepository
    {
        public UserProxy? Create(UserProxy userProxy)
        {
            _context.UserProxies.Add(userProxy);
            if (!Save()) return null;
            return userProxy;
        }

        public bool Delete(UserProxy userProxy)
        {
            _context.Remove(userProxy);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.UserProxies.Any(up => up.Id == id);
        }

        public ICollection<UserProxy> GetAll()
        {
            return [.. _context.UserProxies];
        }

        public UserProxy? GetById(Guid id)
        {
            return _context.UserProxies.Find(id);
        }

        public ICollection<UserProxy> GetByUserId(Guid id)
        {
            return [.. _context.UserProxies.Where(up => up.User.Id == id)];
        }

        public UserProxy? Update(UserProxy userProxy)
        {
            UserProxy? existing = this.GetById(userProxy.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(userProxy);

            if (!Save()) return null;
            return existing;
        }
    }
}
