using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface ICustomPageRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<CustomPage> GetByNetworkId(Guid networkId);
        CustomPage? GetById(Guid id);
        CustomPage Create(CustomPage customPage);
        CustomPage Update(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage);
        void Delete(CustomPage customPage);
    }
}