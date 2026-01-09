﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkAccessService(
        INetworkAccessRepository repository,
        INetworkUserService networkUserService,
        INetworkService networkService,
        INetworkUserAccessService networkUserAccessService,
        IAccessService accessService,
        IMapper mapper) : INetworkAccessService
    {
        private async Task HandleNetworkAccessUpdateAsync(Guid networkId)
        {
            await networkUserService.NetworkAccessUpdateAsync(networkId);
        }

        public async Task<ICollection<NetworkAccess>> GetAllAsync()
        {
            var networkAccesses = await repository.GetAllAsync();
            return networkAccesses;
        }

        public async Task<ICollection<NetworkAccess>> GetByNetworkIdAsync(Guid id)
        {
            if (await networkService.GetByIdAsync(id) == null) throw new NotFoundHttpException("Network not found.");

            var networkAccesses = await repository.GetByNetworkIdAsync(id);
            return networkAccesses;
        }

        public async Task<ICollection<NetworkAccess>> GetByAccessIdAsync(Guid id)
        {
            if (await accessService.GetByIdAsync(id) == null) throw new NotFoundHttpException("Access not found.");

            var networkAccesses = await repository.GetByAccessIdAsync(id);
            return networkAccesses;
        }

        public async Task<NetworkAccess?> GetByIdsAsync(Guid networkId, Guid accessId)
        {
            var networkAccesses = await repository.GetByIdsAsync(networkId, accessId);
            return networkAccesses;
        }

        public async Task<NetworkAccess> CreateAsync(Guid networkId, Guid accessId, CreateNetworkAccessDto createNetworkAccess)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found.");
            if (await accessService.GetByIdAsync(accessId) == null) throw new NotFoundHttpException("Access not found.");

            if (await repository.ExistsAsync(networkId, accessId))
                throw new ConflictHttpException("NetworkAccess already exists.");

            await HandleNetworkAccessUpdateAsync(networkId);

            var networkAccess = mapper.Map<NetworkAccess>(createNetworkAccess);

            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;

            networkAccess = await repository.CreateAsync(networkAccess);
            await networkUserAccessService.PopulateAsync(networkId, accessId);

            return networkAccess;
        }

        public async Task<NetworkAccess> UpdateAsync(Guid networkId, Guid accessId, UpdateNetworkAccessDto updateNetworkAccess)
        {
            var oldNetworkAccess = await GetByIdsAsync(networkId, accessId);
            if (oldNetworkAccess == null) throw new NotFoundHttpException();

            await HandleNetworkAccessUpdateAsync(networkId);

            var networkAccess = mapper.Map<NetworkAccess>(updateNetworkAccess);
            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;

            networkAccess = await repository.UpdateAsync(networkAccess);
            return networkAccess;
        }

        public async Task DeleteByIdsAsync(Guid networkId, Guid accessId)
        {
            var networkAccess = await repository.GetByIdsAsync(networkId, accessId);
            if (networkAccess != null) await HandleNetworkAccessUpdateAsync(networkId);
            
            await networkUserAccessService.DepopulateAsync(networkId, accessId);
            
            if (networkAccess != null) await repository.DeleteAsync(networkAccess);
        }
    }
}