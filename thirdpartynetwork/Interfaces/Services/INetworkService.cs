using thirdpartynetwork.Dtos;
using thirdpartynetwork.Dtos.Network;

namespace thirdpartynetwork.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        ICollection<NetworkDto> GetAll();
        NetworkDto? GetById(Guid id);
        NetworkDto? Create(CreateNetworkDto createNetworkDto);
        NetworkDto? Update(Guid id, UpdateNetworkDto updateNetworkDto);
        bool DeleteById(Guid id);
    }
}
