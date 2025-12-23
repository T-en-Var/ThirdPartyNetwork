using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IConfigurationService : IBaseService
    {
        ICollection<Configuration> GetAll();
        ICollection<Configuration> GetByNetworkId(Guid networkId);
        Configuration? GetById(Guid networkId, Guid configurationId);
        Configuration Create(Guid networkId, CreateConfigurationDto createConfiguration);
        Configuration Update(Guid networkId, Guid configurationId, UpdateConfigurationDto updateConfiguration);
        void DeleteById(Guid networkId, Guid configurationId);
    }
}
