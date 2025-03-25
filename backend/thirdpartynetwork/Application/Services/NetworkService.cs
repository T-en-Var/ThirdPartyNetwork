using AutoMapper;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(INetworkRepository repository, IMapper mapper) : INetworkService
    {
        public ICollection<Network> GetAll()
        {
            var networks = repository.GetAll();
            return networks;
        }

        public Network? GetById(Guid id)
        {
            var network = repository.GetById(id);
            return network;
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
            var mainNetwork = repository.GetByName(Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");
            
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
