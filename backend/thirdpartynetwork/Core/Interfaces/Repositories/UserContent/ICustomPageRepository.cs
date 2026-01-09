using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface ICustomPageRepository : IUserContentBaseRepository<CustomPage>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<CustomPage>> GetByNetworkIdAsync(Guid networkId);
        Task<CustomPage?> GetByIdAsync(Guid id);
        Task<CustomPage> CreateAsync(CustomPage customPage);
        Task<CustomPage> UpdateAsync(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage);
        Task DeleteAsync(CustomPage customPage);
    }
}