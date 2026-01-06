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

        protected override IQueryable<Blog> GetQueryable()
        {
            return _context.Blogs.Include(x => x.CoverImage).AsQueryable();
        }

        public Blog? GetByNetworkIdAndSlug(Guid networkId, string slug)
        {
            return GetQueryable().FirstOrDefault(x => x.NetworkId == networkId && x.Slug == slug);
        }

        public Blog Create(Blog blog)
        {
            _context.Add(blog);
            SaveAndUpdate(blog);
            return blog;
        }

        public void Delete(Blog blog)
        {
            _context.Remove(blog);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Set<Blog>().Any(b => b.Id == id);
        }

        public Blog? GetById(Guid id)
        {
            return GetQueryable().FirstOrDefault(x => x.Id == id);
        }

        public ICollection<Blog> GetByNetworkId(Guid networkId)
        {
            return GetAccessibleQueryable(networkId).Where(b => b.Network.Id == networkId).ToList();
        }

        public Blog Update(Guid blogId, UpdateBlogDto updateBlog)
        {
            var existing = GetById(blogId);
            if (existing == null) throw new Exception("Blog not found");

            mapper.Map(updateBlog, existing);

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
