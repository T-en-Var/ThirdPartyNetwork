using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        Task<ICollection<Network>> GetAllAsync();
        Task<Network?> GetByIdAsync(Guid networkId, bool bypass = false);
        Task<Network?> GetDetailsByIdAsync(Guid networkId);
        Task<Network> CreateAsync(CreateNetworkDto createNetworkDto);
        Task<Network> UpdateAsync(Guid id, UpdateNetworkDto updateNetworkDto);
        Task DeleteByIdAsync(Guid id);
        Task<Network> GetMainNetworkAsync();
        IQueryable<Network> GetMainNetworkQueryable();
        IQueryable<Network> GetByIdQueryable(Guid networkId);
    }
}
