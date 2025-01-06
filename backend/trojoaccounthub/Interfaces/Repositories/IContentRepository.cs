using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IContentRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Content> GetAll();
        ICollection<Content> GetByNetworkId(Guid networkId);
        Content? GetById(Guid id);
        Content? Create(Content content);
        Content? Update(Content content);
        bool Delete(Content content);
    }
}
