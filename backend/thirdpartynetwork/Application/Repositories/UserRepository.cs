using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<User> GetQueryable()
        {
            return _context.Users
                .Include(u => u.UserProxies)
                .AsQueryable();
        }

        public bool Exists(Guid id)
        {
            return _context.Users.Any(u => u.Id == id);
        }

        public ICollection<User> GetAll()
        {
            return [.. GetQueryable()];
        }

        public User? GetById(Guid id)
        {
            return GetQueryable()
                .FirstOrDefault(u => u.Id == id);
        }
        
        public User Create(User user)
        {
            _context.Users.Add(user);
            Save();
            return user;
        }

        public User Update(User user)
        {
            var existing = GetById(user.Id);
            if (existing == null) throw new Exception("User not found");

            _context.Entry(existing).CurrentValues.SetValues(user);

            Save();
            return existing;
        }

        public bool Delete(User user)
        {
            _context.Remove(user);
            return Save();
        }
    }
}
