using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IContentService : IBaseService
    {
        ICollection<Content> GetAll();
        ICollection<Content> GetByNetworkId(Guid networkId);
        Content? GetById(Guid id);
        Content Create(Guid networkId, CreateContentDto createContent);
        Content UpdateById(Guid id, UpdateContentDto updateContent);
        bool DeleteById(Guid id);
    }
}
