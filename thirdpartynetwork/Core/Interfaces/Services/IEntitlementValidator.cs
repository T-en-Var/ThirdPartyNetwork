using System.Text.Json;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IEntitlementValidator
{
    Task<bool> CheckUserAddNetworkFileEntitlementAsync(Guid userProxyId, Guid networkId, IFormFile file);
    Task<bool> CheckUserAddBlogEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddConfigurationEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageBlockEntitlementAsync(Guid userProxyId, Guid networkId, Guid customPageId, JsonElement pageBlockData);
    
}