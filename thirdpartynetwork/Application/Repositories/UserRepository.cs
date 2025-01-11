using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
    {
        private readonly DataContext _context = context;

        public User Create(User user)
        {
            _context.Users.Add(user);
            var x = Save();
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
            return [.. _context.Users.Include(u => u.Proxies)];
        }

        public User? GetById(Guid id)
        {
            return _context.Users.Include(u => u.Proxies).FirstOrDefault(u => u.Id == id);
        }

        public User Update(User user)
        {
            var existing = this.GetById(user.Id);
            if (existing == null) throw new Exception("User not found");

            _context.Entry(existing).CurrentValues.SetValues(user);

            Save();
            return existing;
        }
    }
}