using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IBlogRepository : IUserContentBaseRepository<Blog>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<Blog>> GetByNetworkIdAsync(Guid networkId);
        Task<Blog?> GetByIdAsync(Guid id);
        Task<Blog?> GetByNetworkIdAndSlugAsync(Guid networkId, string slug);
        Task<Blog> CreateAsync(Blog blog);
        Task<Blog> UpdateAsync(Guid blogId, UpdateBlogDto updateBlog);
        Task DeleteAsync(Blog blog);
    }
}
