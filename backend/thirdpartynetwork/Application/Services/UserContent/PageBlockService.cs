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

        public async Task<ICollection<PageBlock>> GetByCustomPageIdAsync(Guid networkId, Guid customPageId)
        {
            var pageBlock = await repository.GetByCustomPageIdAsync(networkId, customPageId);
            return pageBlock;
        }

        public async Task<PageBlock?> GetByIdAsync(Guid networkId, Guid pageBlockId)
        {
            var pageBlock = await repository.GetByIdAsync(pageBlockId);
            return pageBlock;
        }

        public async Task<PageBlock> CreateAsync(Guid networkId, Guid customPageId, CreatePageBlockDto createPageBlock)
        {
            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");
            
            var customPage = await customPageService.GetByIdAsync(networkId, customPageId);
            if (customPage == null) throw new NotFoundHttpException("CustomPage not found");

            var userProxy = await identityService.GetUserProxyAsync();
            var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            var pageBlock = mapper.Map<PageBlock>(createPageBlock);
            pageBlock.NetworkId = networkId;
            pageBlock.CustomPageId = customPageId;
            pageBlock.Author = networkUser;

            pageBlock = await repository.CreateAsync(pageBlock);
            return pageBlock;
        }

        public async Task<PageBlock> UpdateAsync(Guid networkId, Guid customPageId, Guid pageBlockId, UpdatePageBlockDto updatePageBlock)
        {
            if (!await repository.ExistsAsync(pageBlockId)) throw new NotFoundHttpException();

            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var pageBlock = await repository.UpdateAsync(pageBlockId, updatePageBlock);
            return pageBlock;
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid customPageId, Guid pageBlockId)
        {
            var pageBlock = await GetByIdAsync(networkId, pageBlockId);
            if (pageBlock != null) await repository.DeleteAsync(pageBlock);
        }
    }
}