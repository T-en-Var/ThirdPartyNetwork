using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IBlogService : IUserContentBaseService<Blog>
    {
        Task<ICollection<Blog>> GetByNetworkIdAsync(Guid networkId);
        Task<Blog?> GetByIdAsync(Guid networkId, Guid blogId);
        Task<Blog?> GetBySlugAsync(Guid networkId, string slug);
        Task<Blog> CreateAsync(Guid networkId, CreateBlogDto blog);
        Task<Blog> UpdateAsync(Guid networkId, Guid blogId, UpdateBlogDto updateBlog);
        Task DeleteByIdAsync(Guid networkId, Guid blogId);
    }
}
