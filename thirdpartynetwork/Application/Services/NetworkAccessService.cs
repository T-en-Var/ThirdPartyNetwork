using AutoMapper;
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
        private void HandleNetworkAccessUpdate(Guid networkId)
        {
            networkUserService.NetworkAccessUpdate(networkId);
        }

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
            Console.WriteLine($"NetworkId: {networkId}, AccessId: {accessId}");
            if (repository.Exists(networkId, accessId))
                throw new ConflictHttpException("NetworkAccess already exists.");

            HandleNetworkAccessUpdate(networkId);

            var networkAccess = mapper.Map<NetworkAccess>(createNetworkAccess);

            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;

            networkAccess = repository.Create(networkAccess);
            networkUserAccessService.Populate(networkId, accessId);

            return networkAccess;
        }

        public NetworkAccess Update(Guid networkId, Guid accessId, UpdateNetworkAccessDto updateNetworkAccess)
        {
            var oldNetworkAccess = GetByIds(networkId, accessId);
            if (oldNetworkAccess == null) throw new NotFoundHttpException();

            HandleNetworkAccessUpdate(networkId);

            var networkAccess = mapper.Map<NetworkAccess>(updateNetworkAccess);
            networkAccess.NetworkId = networkId;
            networkAccess.AccessId = accessId;

            networkAccess = repository.Update(networkAccess);
            return networkAccess;
        }

        public void DeleteByIds(Guid networkId, Guid accessId)
        {
            var networkAccess = repository.GetByIds(networkId, accessId);
            if (networkAccess != null) HandleNetworkAccessUpdate(networkId);
            
            networkUserAccessService.Depopulate(networkId, accessId);
            
            if (networkAccess != null) repository.Delete(networkAccess);
        }
    }
}