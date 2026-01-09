﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserAccessService(
        INetworkUserAccessRepository repository, 
        INetworkService networkService, 
        INetworkUserService networkUserService, 
        INetworkAccessRepository networkAccessRepository, 
        IAccessService accessService, 
        IMapper mapper) : INetworkUserAccessService
    {
        public async Task<ICollection<NetworkUserAccess>> GetAllAsync()
        {
            var networkUserAccesses = await repository.GetAllAsync();
            return networkUserAccesses;
        }

        public async Task<ICollection<NetworkUserAccess>> GetByNetworkIdAsync(Guid networkId, Guid networkUserId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserAccesses = await repository.GetByNetworkUserIdAsync(networkUserId);
            return networkUserAccesses;
        }

        public async Task<ICollection<NetworkUserAccess>> GetByAccessIdAsync(Guid accessId)
        {
            if (await accessService.GetByIdAsync(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserAccesses = await repository.GetByAccessIdAsync(accessId);
            return networkUserAccesses;
        }

        public async Task PopulateAsync(Guid networkId, Guid accessId)
        {
            await repository.PopulateAsync(networkId, accessId);
        }

        public async Task DepopulateAsync(Guid networkId, Guid accessId)
        {
            await repository.DepopulateAsync(networkId, accessId);
        }

        public async Task<NetworkUserAccess?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid accessId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (await accessService.GetByIdAsync(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserPermission = await repository.GetByIdsAsync(networkUserId, accessId);
            return networkUserPermission;
        }

        public async Task<NetworkUserAccess> CreateAsync(Guid networkId, Guid networkUserId, Guid accessId, CreateNetworkUserAccessDto createNetworkUserPermission)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (await accessService.GetByIdAsync(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            if (await repository.ExistsAsync(networkUserId, accessId)) throw new ConflictHttpException();
            
            var networkUserAccess = mapper.Map<NetworkUserAccess>(createNetworkUserPermission);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = accessId;
            
            networkUserAccess = await repository.CreateAsync(networkUserAccess);

            await MarkAccessCompleteAsync(networkUserId);
            
            return networkUserAccess;
        }

        public async Task<NetworkUserAccess> UpdateAsync(Guid networkId, Guid networkUserId, Guid accessId, UpdateNetworkUserAccessDto updateNetworkUserAccess)
        {
            if (!await repository.ExistsAsync(networkUserId, accessId)) throw new NotFoundHttpException();
            
            var networkAccess = await networkAccessRepository.GetByIdsAsync(networkId, accessId);
            if (networkAccess!.IsRequired && !updateNetworkUserAccess.IsAccepted) throw new BadHttpRequestException("Access is required by this network.");

            var networkUserAccess = mapper.Map<NetworkUserAccess>(updateNetworkUserAccess);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = accessId;
            
            networkUserAccess = await repository.UpdateAsync(networkUserAccess);
            
            await MarkAccessCompleteAsync(networkUserId);
            
            return networkUserAccess;
        }

        private async Task MarkAccessCompleteAsync(Guid networkUserId)
        {
            await repository.MarkAccessCompleteAsync(networkUserId);
        }

        public async Task DeleteByIdsAsync(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            var networkUserPermission = await GetByIdsAsync(networkId, networkUserId, permissionId);
            if (networkUserPermission != null) await repository.DeleteAsync(networkUserPermission);
        }
    }
}
