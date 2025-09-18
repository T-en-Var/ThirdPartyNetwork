using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IPageBlockRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<PageBlock> GetAll();
        ICollection<PageBlock> GetByCustomPageId(Guid customPageId);
        ICollection<PageBlock> GetByParentPageId(Guid parentPageId);
        PageBlock? GetById(Guid id);
        PageBlock Create(PageBlock pageBlock);
        PageBlock Update(Guid pageBlockId, UpdatePageBlockDto updatePageBlock);
        void Delete(PageBlock pageBlock);
    }
}