using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        ICollection<Network> GetAll();
        Network? GetById(Guid networkId);
        Network? GetDetailsById(Guid networkId);
        Network Create(CreateNetworkDto createNetworkDto);
        Network Update(Guid id, UpdateNetworkDto updateNetworkDto);
        bool DeleteById(Guid id);
        Network GetMainNetwork();
        IQueryable<Network> GetMainNetworkQueryable();
        IQueryable<Network> GetByIdQueryable(Guid networkId);
    }
}
