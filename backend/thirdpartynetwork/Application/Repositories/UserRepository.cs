﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await GetQueryable().AsNoTracking().ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await SaveAndUpdateAsync(user);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var existing = await GetByIdAsync(user.Id);
            if (existing == null) throw new Exception("User not found");

            _context.Entry(existing).CurrentValues.SetValues(user);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(User user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
