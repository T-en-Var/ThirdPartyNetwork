using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Exceptions;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(INetworkRepository networkRepository, IMapper mapper) : INetworkService
    {
        public NetworkDto? Create(CreateNetworkDto createNetworkDto)
        {
            var nameFound = networkRepository.GetByName(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            Network? network = mapper.Map<Network>(createNetworkDto);
            network = networkRepository.Create(network);
            
            return mapper.Map<NetworkDto>(network);
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public NetworkDto? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkDto? Update(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            throw new NotImplementedException();
        }
    }
}
