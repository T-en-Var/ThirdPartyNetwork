using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class CustomPageRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<CustomPage>(context, verifier), ICustomPageRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadCustomPage;

        protected override Task<IQueryable<CustomPage>> GetQueryableAsync()
        {
            var query = _context.CustomPages
                .Include(x => x.Pages)
                .AsQueryable();
            
            return Task.FromResult(query);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.CustomPages.AnyAsync(c => c.Id == id);
        }

        public async Task<CustomPage?> GetByIdAsync(Guid id)
        {
            return await (await GetQueryableAsync()).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ICollection<CustomPage>> GetByNetworkIdAsync(Guid networkId)
        {
            return await (await GetAccessibleQueryableAsync(networkId))
                .AsNoTracking().Where(c => c.Network.Id == networkId).ToListAsync();
        }

        public async Task<CustomPage> CreateAsync(CustomPage customPage)
        {
            await _context.CustomPages.AddAsync(customPage);
            await SaveAndUpdateAsync(customPage);
            return customPage;
        }

        public async Task<CustomPage> UpdateAsync(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage)
        {
            var existing = await GetByIdAsync(customPageId);
            if (existing == null) throw new Exception("CustomPage not found");

            mapper.Map(updateCustomPage, existing);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(CustomPage customPage)
        {
            _context.Remove(customPage);
            await _context.SaveChangesAsync();
        }
    }
}