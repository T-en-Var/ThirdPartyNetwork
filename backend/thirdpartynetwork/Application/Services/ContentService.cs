using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class ContentService(IContentRepository repository, INetworkService networkService, IMapper mapper) : IContentService
    {
        public ICollection<Content> GetAll()
        {
            var content = repository.GetAll();
            return content;
        }

        public ICollection<Content> GetByNetworkId(Guid networkId)
        {
            var content = repository.GetByNetworkId(networkId);
            return content;
        }

        public Content? GetById(Guid contentId)
        {
            var content = repository.GetById(contentId);
            return content;
        }

        public Content Create(Guid networkId, CreateContentDto createContent)
        {
            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");
            
            var content = mapper.Map<Content>(createContent);
            content.Network = network;
            
            content = repository.Create(content);
            return content;
        }

        public Content Update(Guid networkId, Guid contentId, UpdateContentDto updateContent)
        {
            if (!repository.Exists(contentId)) throw new NotFoundHttpException();
            
            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");
            
            var content = mapper.Map<Content>(updateContent);
            content.Network = network;
            content.Id = contentId;
            
            content = repository.Update(content);
            return content;
        }

        public void DeleteById(Guid contentId)
        {
            var content = GetById(contentId);
            if (content != null) repository.Delete(content);
        }
    }
}
