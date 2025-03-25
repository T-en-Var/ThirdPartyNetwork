using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkAccessService(INetworkAccessRepository repository, INetworkService networkService, IAccessService accessService, IMapper mapper) : INetworkAccessService
    {
        public ICollection<NetworkAccess> GetAll()
        {
            var networkAccesses = repository.GetAll();
            return networkAccesses;
        }

        public ICollection<NetworkAccess> GetByNetworkId(Guid id)
        {
            if (networkService.GetById(id) == null) throw new NotFoundHttpException("Network not found.");
            
            var networkAccesses = repository.GetByNetworkId(id);
            return networkAccesses;
        }

        public ICollection<NetworkAccess> GetByAccessId(Guid id)
        {
            if (accessService.GetById(id) == null) throw new NotFoundHttpException("Access not found.");
            
            var networkAccesses = repository.GetByAccessId(id);
            return networkAccesses;
        }

        public NetworkAccess? GetByIds(Guid networkId, Guid accessId)
        {
            var networkAccesses = repository.GetByIds(networkId, accessId);
            return networkAccesses;
        }

        public NetworkAccess Create(Guid networkId, Guid accessId, CreateNetworkAccessDto createNetworkAccess)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found.");
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found.");
            
            if (repository.Exists(networkId, accessId)) throw new ConflictHttpException("NetworkAccess already exists.");
            
            var networkAccess = mapper.Map<NetworkAccess>(createNetworkAccess);
            
            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;
            
            networkAccess = repository.Create(networkAccess);
            
            return networkAccess;
        }

        public NetworkAccess Update(Guid networkId, Guid accessId, UpdateNetworkAccessDto updateNetworkAccess)
        {
            if (!repository.Exists(networkId, accessId)) throw new NotFoundHttpException();
            
            var networkAccess = mapper.Map<NetworkAccess>(updateNetworkAccess);
            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;
            
            networkAccess = repository.Update(networkAccess);
            return networkAccess;
        }

        public bool DeleteByIds(Guid networkId, Guid permissiodId)
        {
            var networkAccess = repository.GetByIds(networkId, permissiodId);
            return networkAccess != null && repository.Delete(networkAccess);
        }
    }
}
