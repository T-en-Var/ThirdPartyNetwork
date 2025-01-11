using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserProxyRepository(DataContext context) : BaseRepository(context), IUserProxyRepository
    {
        private readonly DataContext _context = context;

        public UserProxy Create(UserProxy userProxy)
        {
            _context.UserProxies.Add(userProxy);
            Save();
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

        public UserProxy Update(UserProxy userProxy)
        {
            var existing = this.GetById(userProxy.Id);
            if (existing == null) throw new Exception("UserProxy not found");

            _context.Entry(existing).CurrentValues.SetValues(userProxy);

            Save();
            return existing;
        }
    }
}
