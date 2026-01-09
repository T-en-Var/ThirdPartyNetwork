using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IConfigurationService : IUserContentBaseService<Configuration>
    {
        Task<ICollection<Configuration>> GetByNetworkIdAsync(Guid networkId);
        Task<Configuration?> GetByIdAsync(Guid networkId, Guid configurationId);
        Task<Configuration> CreateAsync(Guid networkId, CreateConfigurationDto createConfiguration);
        Task<Configuration> UpdateAsync(Guid networkId, Guid configurationId, UpdateConfigurationDto updateConfiguration);
        Task DeleteByIdAsync(Guid networkId, Guid configurationId);
    }
}
