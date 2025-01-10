using thirdpartynetwork.Application.Dtos;

namespace thirdpartynetwork.Core.Interfaces.Services
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
