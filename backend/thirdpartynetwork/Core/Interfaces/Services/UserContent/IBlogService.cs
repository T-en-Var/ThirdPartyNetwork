using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IBlogService : IUserContentBaseService<Blog>
    {
        ICollection<Blog> GetByNetworkId(Guid networkId);
        Blog? GetById(Guid networkId, Guid blogId);
        Blog? GetBySlug(Guid networkId, string slug);
        Blog Create(Guid networkId, CreateBlogDto blog);
        Blog Update(Guid networkId, Guid blogId, UpdateBlogDto updateBlog);
        void DeleteById(Guid networkId, Guid blogId);
    }
}
