﻿using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class AccessRepository(DataContext context) : BaseRepository(context), IAccessRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<Access> GetQueryable()
        {
            var query = _context.Accesses
                .AsQueryable();
            
            // I'll skip this for now.
            
            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Accesses.AnyAsync(p => p.Id == id);
        }

        public async Task<ICollection<Access>> GetAllAsync()
        {
            return await GetQueryable().AsNoTracking().ToListAsync();
        }

        public async Task<Access?> GetByIdAsync(Guid id)
        {
            return await GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Access> CreateAsync(Access access)
        {
            await _context.Accesses.AddAsync(access);
            await SaveAndUpdateAsync(access);
            return access;
        }

        public async Task<Access> UpdateAsync(Access access)
        {
            var existing = await GetByIdAsync(access.Id);
            if (existing == null) throw new Exception("Access not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Access is system protected");

            _context.Entry(existing).CurrentValues.SetValues(access);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(Access access)
        {
            if (access.IsSystemProtected) throw new UnauthorizedHttpException("Access is system protected");

            _context.Remove(access);
            await _context.SaveChangesAsync();
        }
    }
}
