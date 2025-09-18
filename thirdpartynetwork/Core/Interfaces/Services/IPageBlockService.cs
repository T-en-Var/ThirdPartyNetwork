using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IPageBlockService : IBaseService
    {
        ICollection<PageBlock> GetAll();
        ICollection<PageBlock> GetByCustomPageId(Guid customPageId);
        ICollection<PageBlock> GetByParentPageId(Guid parentPageId);
        PageBlock? GetById(Guid networkId, Guid pageBlockId);
        PageBlock Create(Guid networkId, Guid customPageId, CreatePageBlockDto createPageBlock);
        PageBlock Update(Guid networkId, Guid customPageId, Guid pageBlockId, UpdatePageBlockDto updatePageBlock);
        void DeleteById(Guid networkId, Guid customPageId, Guid pageBlockId);
    }
}