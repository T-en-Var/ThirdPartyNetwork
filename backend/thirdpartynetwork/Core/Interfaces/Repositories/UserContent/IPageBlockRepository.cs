using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IPageBlockRepository : IUserContentBaseRepository<PageBlock>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<PageBlock>> GetByCustomPageIdAsync(Guid networkId, Guid customPageId);
        Task<PageBlock?> GetByIdAsync(Guid id);
        Task<PageBlock> CreateAsync(PageBlock pageBlock);
        Task<PageBlock> UpdateAsync(Guid pageBlockId, UpdatePageBlockDto updatePageBlock);
        Task DeleteAsync(PageBlock pageBlock);
    }
}