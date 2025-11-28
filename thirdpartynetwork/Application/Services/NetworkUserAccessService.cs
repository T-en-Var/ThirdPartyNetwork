using AutoMapper;
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
        public ICollection<NetworkUserAccess> GetAll()
        {
            var networkUserAccesses = repository.GetAll();
            return networkUserAccesses;
        }

        public ICollection<NetworkUserAccess> GetByNetworkId(Guid networkId, Guid networkUserId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserAccesses = repository.GetByNetworkUserId(networkUserId);
            return networkUserAccesses;
        }

        public ICollection<NetworkUserAccess> GetByAccessId(Guid accessId)
        {
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserAccesses = repository.GetByAccessId(accessId);
            return networkUserAccesses;
        }

        public void Populate(Guid networkId, Guid accessId)
        {
            repository.Populate(networkId, accessId);
        }

        public void Depopulate(Guid networkId, Guid accessId)
        {
            repository.Depopulate(networkId, accessId);
        }

        public NetworkUserAccess? GetByIds(Guid networkId, Guid networkUserId, Guid accessId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserPermission = repository.GetByIds(networkUserId, accessId);
            return networkUserPermission;
        }

        public NetworkUserAccess Create(Guid networkId, Guid networkUserId, Guid accessId, CreateNetworkUserAccessDto createNetworkUserPermission)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            if (repository.Exists(networkUserId, accessId)) throw new ConflictHttpException();
            
            var networkUserAccess = mapper.Map<NetworkUserAccess>(createNetworkUserPermission);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = accessId;
            
            networkUserAccess = repository.Create(networkUserAccess);

            MarkAccessComplete(networkUserId);
            
            return networkUserAccess;
        }

        public NetworkUserAccess Update(Guid networkId, Guid networkUserId, Guid accessId, UpdateNetworkUserAccessDto updateNetworkUserAccess)
        {
            if (!repository.Exists(networkUserId, accessId)) throw new NotFoundHttpException();
            
            var networkAccess = networkAccessRepository.GetByIds(networkId, accessId);
            if (networkAccess!.IsRequired && !updateNetworkUserAccess.IsAccepted) throw new BadHttpRequestException("Access is required by this network.");

            var networkUserAccess = mapper.Map<NetworkUserAccess>(updateNetworkUserAccess);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = accessId;
            
            networkUserAccess = repository.Update(networkUserAccess);
            
            MarkAccessComplete(networkUserId);
            
            return networkUserAccess;
        }

        private void MarkAccessComplete(Guid networkUserId)
        {
            repository.MarkAccessComplete(networkUserId);
        }

        public void DeleteByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            var networkUserPermission = GetByIds(networkId, networkUserId, permissionId);
            if (networkUserPermission != null) repository.Delete(networkUserPermission);
        }
    }
}
