using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Exceptions;
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
            var network = repository.GetById(id);
            if (network == null) throw new NotFoundHttpException();
            
            var obj = mapper.Map(updateNetworkDto, network);
            obj.Id = id;
            
            return repository.Update(obj);
        }

        public bool DeleteById(Guid id)
        {
            var network = repository.GetById(id);
            return network != null && repository.Delete(network);
        }
    }
}
