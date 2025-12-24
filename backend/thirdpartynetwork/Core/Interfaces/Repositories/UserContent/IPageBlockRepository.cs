using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IPageBlockRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<PageBlock> GetByCustomPageId(Guid networkId, Guid customPageId);
        PageBlock? GetById(Guid id);
        PageBlock Create(PageBlock pageBlock);
        PageBlock Update(Guid pageBlockId, UpdatePageBlockDto updatePageBlock);
        void Delete(PageBlock pageBlock);
    }
}