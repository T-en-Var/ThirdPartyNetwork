using trojoaccounthub.Dtos;

namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        ICollection<NetworkDto> GetAll();
        NetworkDto? GetById(int id);
        NetworkDto? Create(CreateNetworkDto createNetworkDto);
        NetworkDto? Update(int id, UpdateNetworkDto updateNetworkDto);
        bool DeleteById(int id);
    }
}
