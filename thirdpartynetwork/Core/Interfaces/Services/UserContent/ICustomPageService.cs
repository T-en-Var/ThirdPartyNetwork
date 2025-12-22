using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface ICustomPageService : IBaseService
    {
        ICollection<CustomPage> GetAll();
        ICollection<CustomPage> GetByNetworkId(Guid networkId);
        CustomPage? GetById(Guid networkId, Guid customPageId);
        CustomPage Create(Guid networkId, CreateCustomPageDto createCustomPage);
        CustomPage Update(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage);
        void DeleteById(Guid networkId, Guid customPageId);
    }
}