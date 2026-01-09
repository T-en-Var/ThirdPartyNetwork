using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface ICustomPageService : IUserContentBaseService<CustomPage>
    {
        Task<ICollection<CustomPage>> GetByNetworkIdAsync(Guid networkId);
        Task<CustomPage?> GetByIdAsync(Guid networkId, Guid customPageId);
        Task<CustomPage> CreateAsync(Guid networkId, CreateCustomPageDto createCustomPage);
        Task<CustomPage> UpdateAsync(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage);
        Task DeleteByIdAsync(Guid networkId, Guid customPageId);
    }
}