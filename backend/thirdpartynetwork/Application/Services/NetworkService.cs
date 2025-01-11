using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(INetworkRepository networkRepository, IMapper mapper) : INetworkService
    {
        public ICollection<Network> GetAll()
        {
            throw new NotImplementedException();
        }

        public Network? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Network Create(CreateNetworkDto createNetworkDto)
        {
            var nameFound = networkRepository.GetByName(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            var network = mapper.Map<Network>(createNetworkDto);
            network = networkRepository.Create(network);
            
            return network;
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
        
        public Network Update(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            throw new NotImplementedException();
        }
    }
}
