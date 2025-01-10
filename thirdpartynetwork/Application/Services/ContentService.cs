namespace thirdpartynetwork.Application.Services
{
    public class ContentService : IContentService
    {
        public ICollection<ContentDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<ContentDto> GetByNetworkId(Guid networkId)
        {
            throw new NotImplementedException();
        }

        public ContentDto? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public ContentDto? Create(Guid networkId, CreateContentDto content)
        {
            throw new NotImplementedException();
        }

        public ContentDto? UpdateById(Guid id, UpdateContentDto content)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
