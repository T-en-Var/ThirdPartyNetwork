using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class ContentService : IContentService
    {
        public ICollection<Content> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<Content> GetByNetworkId(Guid networkId)
        {
            throw new NotImplementedException();
        }

        public Content? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Content Create(Guid networkId, CreateContentDto createContent)
        {
            throw new NotImplementedException();
        }

        public Content UpdateById(Guid id, UpdateContentDto updateContent)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
