using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        ICollection<Network> GetAll();
        Network? GetById(Guid id);
        Network Create(CreateNetworkDto createNetworkDto);
        Network Update(Guid id, UpdateNetworkDto updateNetworkDto);
        bool DeleteById(Guid id);
        Network GetMainNetwork();
    }
}
