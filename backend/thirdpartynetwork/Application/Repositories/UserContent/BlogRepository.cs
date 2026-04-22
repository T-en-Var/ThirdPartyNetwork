using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class BlogRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<Blog>(context, verifier), IBlogRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadBlog;

        protected override Task<IQueryable<Blog>> GetQueryableAsync()
        {
            var query = _context.Blogs
                .Include(x => x.CoverImage)
                .Include(x => x.Author)
                .ThenInclude(x => x.UserProxy)
                .Include(x => x.Author)
                .ThenInclude(x => x.NetworkUserAccesses)
                .ThenInclude(x => x.Access)
                .AsQueryable();
            
            return Task.FromResult(query);
        }

        public async Task<Blog?> GetByNetworkIdAndSlugAsync(Guid networkId, string slug)
        {
            return await (await GetQueryableAsync()).FirstOrDefaultAsync(x => x.NetworkId == networkId && x.Slug == slug);
        }

        public async Task<Blog> CreateAsync(Blog blog)
        {
            await _context.AddAsync(blog);
            await SaveAndUpdateAsync(blog);
            return blog;
        }

        public async Task DeleteAsync(Blog blog)
        {
            _context.Remove(blog);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Set<Blog>().AnyAsync(b => b.Id == id);
        }

        public async Task<Blog?> GetByIdAsync(Guid id)
        {
            return await (await GetQueryableAsync()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Blog> UpdateAsync(Guid blogId, UpdateBlogDto updateBlog)
        {
            var existing = await GetByIdAsync(blogId);
            if (existing == null) throw new Exception("Blog not found");

            mapper.Map(updateBlog, existing);

            await SaveAndUpdateAsync(existing);
            return existing;
        }
    }
}
