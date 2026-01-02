using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IBlogRepository : IUserContentBaseRepository<Blog>
    {
        bool Exists(Guid id);
        ICollection<Blog> GetByNetworkId(Guid networkId);
        Blog? GetById(Guid id);
        Blog? GetByNetworkIdAndSlug(Guid networkId, string slug);
        Blog Create(Blog blog);
        Blog Update(Guid blogId, UpdateBlogDto updateBlog);
        void Delete(Blog blog);
    }
}
