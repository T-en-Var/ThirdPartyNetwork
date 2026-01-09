using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class PageBlockRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<PageBlock>(context, verifier), IPageBlockRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadPageBlock;

        protected override Task<IQueryable<PageBlock>> GetQueryableAsync()
        {
            var query = _context.PageBlocks
                .AsQueryable();
            
            return Task.FromResult(query);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.PageBlocks.AnyAsync(c => c.Id == id);
        }

        public async Task<ICollection<PageBlock>> GetByCustomPageIdAsync(Guid networkId, Guid customPageId)
        {
            return await (await GetQueryableAsync())
                .AsNoTracking()
                .Where(p => p.CustomPageId == customPageId).ToListAsync();
        }

        public async Task<PageBlock?> GetByIdAsync(Guid id)
        {
            return await _context.PageBlocks.FindAsync(id);
        }

        public async Task<PageBlock> CreateAsync(PageBlock pageBlock)
        {
            await _context.PageBlocks.AddAsync(pageBlock);
            await SaveAndUpdateAsync(pageBlock);
            return pageBlock;
        }

        public async Task<PageBlock> UpdateAsync(Guid pageBlockId, UpdatePageBlockDto updatePageBlock)
        {
            var existing = await GetByIdAsync(pageBlockId);
            if (existing == null) throw new Exception("PageBlock not found");

            mapper.Map(updatePageBlock, existing); 

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(PageBlock pageBlock)
        {
            _context.Remove(pageBlock);
            await _context.SaveChangesAsync();
        }
    }
}