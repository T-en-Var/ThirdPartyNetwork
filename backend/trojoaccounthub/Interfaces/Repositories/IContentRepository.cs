using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface IContentRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<Content> GetAll();
        ICollection<Content> GetByNetworkId(int networkId);
        Content? GetById(int id);
        Content? Create(Content content);
        Content? Update(Content content);
        bool Delete(Content content);
    }
}
