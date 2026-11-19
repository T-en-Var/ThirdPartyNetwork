using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IConfigurationRepository : IUserContentBaseRepository<Configuration>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<Configuration>> GetByNetworkIdAsync(Guid networkId);
        Task<Configuration?> GetByIdAsync(Guid id);
        Task<Configuration> CreateAsync(Configuration configuration);
        Task<Configuration> UpdateAsync(Guid configurationId, UpdateConfigurationDto updateConfiguration);
        Task DeleteAsync(Configuration configuration);
    }
}