using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class PageBlockService(
        IPageBlockRepository repository,
        INetworkService networkService,
        ICustomPageService customPageService,
        IIdentityService identityService,
        INetworkUserService networkUserService,
        IMapper mapper) : UserContentBaseService<PageBlock>(repository, networkUserService), IPageBlockService
    {
        private readonly INetworkUserService _networkUserService = networkUserService;

        public ICollection<PageBlock> GetByCustomPageId(Guid networkId, Guid customPageId)
        {
            var pageBlock = repository.GetByCustomPageId(networkId, customPageId);
            return pageBlock;
        }

        public PageBlock? GetById(Guid networkId, Guid pageBlockId)
        {
            var pageBlock = repository.GetById(pageBlockId);
            return pageBlock;
        }

        public PageBlock Create(Guid networkId, Guid customPageId, CreatePageBlockDto createPageBlock)
        {
            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");
            
            var customPage = customPageService.GetById(networkId, customPageId);
            if (customPage == null) throw new NotFoundHttpException("CustomPage not found");

            var userProxy = identityService.GetUserProxy();
            var networkUser = _networkUserService.GetByIds(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            var pageBlock = mapper.Map<PageBlock>(createPageBlock);
            pageBlock.CustomPage = customPage;
            pageBlock.Author = networkUser;

            pageBlock = repository.Create(pageBlock);
            return pageBlock;
        }

        public PageBlock Update(Guid networkId, Guid customPageId, Guid pageBlockId, UpdatePageBlockDto updatePageBlock)
        {
            if (!repository.Exists(pageBlockId)) throw new NotFoundHttpException();

            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var pageBlock = repository.Update(pageBlockId, updatePageBlock);
            return pageBlock;
        }

        public void DeleteById(Guid networkId, Guid customPageId, Guid pageBlockId)
        {
            var pageBlock = GetById(networkId, pageBlockId);
            if (pageBlock != null) repository.Delete(pageBlock);
        }
    }
}