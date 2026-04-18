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
        private static ICollection<NetworkUserAccess> MultiApplyDefaults(ICollection<NetworkUserAccess> networkUserAccesses)
        {
            foreach (var networkUserAccess in networkUserAccesses) SingleApplyDefaults(networkUserAccess);

            return networkUserAccesses;
        }

        private static NetworkUserAccess? SingleApplyDefaults(NetworkUserAccess? networkUserAccess)
        {
            if (networkUserAccess == null) return null;
            networkUserAccess.NetworkUser.UserProxy.ApplyDefaults();

            return networkUserAccess;
        }

        public async Task<ICollection<NetworkUserAccess>> GetAllAsync()
        {
            var networkUserAccesses = await repository.GetAllAsync();
            return MultiApplyDefaults(networkUserAccesses);
        }

        public async Task<ICollection<NetworkUserAccess>> GetByNetworkIdAsync(Guid networkId, Guid networkUserId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserAccesses = await repository.GetByNetworkUserIdAsync(networkUserId);
            return MultiApplyDefaults(networkUserAccesses);
        }

        public async Task<ICollection<NetworkUserAccess>> GetByAccessIdAsync(Guid accessId)
        {
            if (await accessService.GetByIdAsync(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserAccesses = await repository.GetByAccessIdAsync(accessId);
            return MultiApplyDefaults(networkUserAccesses);
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
            
            var networkUserAccess = await repository.GetByIdsAsync(networkUserId, accessId);
            return SingleApplyDefaults(networkUserAccess);
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
            
            return SingleApplyDefaults(networkUserAccess)!;
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
            
            return SingleApplyDefaults(networkUserAccess)!;
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
