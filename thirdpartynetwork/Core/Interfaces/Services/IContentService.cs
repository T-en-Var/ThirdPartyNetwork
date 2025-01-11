using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IContentService : IBaseService
    {
        ICollection<Content> GetAll();
        ICollection<Content> GetByNetworkId(Guid networkId);
        Content? GetById(Guid id);
        Content Create(Guid networkId, CreateContentDto content);
        Content UpdateById(Guid id, UpdateContentDto content);
        bool DeleteById(Guid id);
    }
}
