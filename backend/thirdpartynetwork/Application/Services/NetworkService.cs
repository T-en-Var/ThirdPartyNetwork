using AutoMapper;
using thirdpartynetwork.Dtos;
using thirdpartynetwork.Exceptions;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Services
{
    public class NetworkService : INetworkService
    {
        private readonly INetworkRepository _networkRepository;
        private readonly IMapper _mapper;

        public NetworkService(INetworkRepository networkRepository, IMapper mapper)
        {
            _networkRepository = networkRepository;
            _mapper = mapper;
        }

        public NetworkDto? Create(CreateNetworkDto createNetworkDto)
        {
            var nameFound = _networkRepository.GetByName(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            Network? network = _mapper.Map<Network>(createNetworkDto);
            network = _networkRepository.Create(network);
            
            return _mapper.Map<NetworkDto>(network);
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
