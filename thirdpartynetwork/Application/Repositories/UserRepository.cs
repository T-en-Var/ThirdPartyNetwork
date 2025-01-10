using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
    {
        private readonly DataContext _context = context;

        public User? Create(User user)
        {
            _context.Users.Add(user);
            if (!Save()) return null;
            return user;
        }

        public bool Delete(User user)
        {
            _context.Remove(user);
            return Save();
        }

        public bool Exists(Guid id)
        {
            return _context.Users.Any(u => u.Id == id);
        }

        public ICollection<User> GetAll()
        {
            return [.. _context.Users];
        }

        public User? GetById(Guid id)
        {
            return _context.Users.Find(id);
        }

        public User? Update(User user)
        {
            User? existing = this.GetById(user.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(user);

            if (!Save()) return null;
            return existing;
        }
    }
}
