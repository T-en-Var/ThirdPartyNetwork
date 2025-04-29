using AutoMapper;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(INetworkRepository repository, IMapper mapper, IIdentityService identityService, IIdentityVerifier identityVerifier) : INetworkService
    {
        private ICollection<Network> FilterNetworks(ICollection<Network> networks)
        {
            var currentUser = identityService.GetUserProxy();
            return identityVerifier.IsSuperAdminOrHasSuperPermission(currentUser.Id, Constants.Permissions.ReadNetwork) 
                ? networks 
                : networks.Where(n => n.IsPublic).ToList();
        }
        
        public ICollection<Network> GetAll()
        {
            var networks = repository.GetAll();
            return FilterNetworks(networks);
        }

        public Network? GetById(Guid id)
        {
            var network = repository.GetById(id);
            if (network == null) return null;
            
            var networks = FilterNetworks([ network ]);
            networks = FilterNetworks(networks);
            return networks.Count > 0 ? network : null;
        }

        public Network? GetDetailsById(Guid id)
        {
            var network = repository.GetDetailsById(id);
            if (network == null) return null;
            
            var currentUser = identityService.GetUserProxy();

            network.NetworkUsers = network.NetworkUsers
                .Where(x => x.UserProxyId == currentUser.Id).ToList();
            
            var networks = FilterNetworks([ network ]);
            networks = FilterNetworks(networks);
            return networks.Count > 0 ? network : null;
        }

        public Network Create(CreateNetworkDto createNetworkDto)
        {
            var nameFound = repository.GetByName(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            var network = mapper.Map<Network>(createNetworkDto);
            network = repository.Create(network);
            
            return network;
        }
        
        public Network Update(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            if (!repository.Exists(id)) throw new NotFoundHttpException();
            
            var network = mapper.Map<Network>(updateNetworkDto);
            network.Id = id;
            
            network = repository.Update(network);
            return network;
        }

        public bool DeleteById(Guid id)
        {
            var network = GetById(id);
            return network != null && repository.Delete(network);
        }

        public Network GetMainNetwork()
        {
            var mainNetwork = repository.GetMainNetwork();
            
            return mainNetwork;
        }

        public IQueryable<Network> GetMainNetworkQueryable()
        {
            var mainNetwork = repository.GetByNameQueryable(Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");
            
            return mainNetwork;
        }

        public IQueryable<Network> GetByIdQueryable(Guid id)
        {
            var network = repository.GetByIdQueryable(id);
            return network;
        }
    }
}
