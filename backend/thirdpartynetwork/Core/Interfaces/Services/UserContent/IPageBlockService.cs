using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IPageBlockService : IUserContentBaseService<PageBlock>
    {
        Task<ICollection<PageBlock>> GetByCustomPageIdAsync(Guid networkId, Guid customPageId);
        Task<PageBlock?> GetByIdAsync(Guid networkId, Guid pageBlockId);
        Task<PageBlock> CreateAsync(Guid networkId, Guid customPageId, CreatePageBlockDto createPageBlock);
        Task<PageBlock> UpdateAsync(Guid networkId, Guid customPageId, Guid pageBlockId, UpdatePageBlockDto updatePageBlock);
        Task DeleteByIdAsync(Guid networkId, Guid customPageId, Guid pageBlockId);
    }
}