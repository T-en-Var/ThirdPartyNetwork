using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IConfigurationRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<Configuration> GetByNetworkId(Guid networkId);
        Configuration? GetById(Guid id);
        Configuration Create(Configuration configuration);
        Configuration Update(Guid configurationId, UpdateConfigurationDto updateConfiguration);
        void Delete(Configuration configuration);
    }
}