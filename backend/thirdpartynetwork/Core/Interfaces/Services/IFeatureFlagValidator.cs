using System.Text.Json;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IFeatureFlagValidator
{
    Task<bool> CheckUserAddNetworkFileFeatureFlagAsync(Guid userProxyId, Guid networkId, IFormFile file);
    Task<bool> CheckUserAddBlogFeatureFlagAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddConfigurationFeatureFlagAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageFeatureFlagAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageBlockFeatureFlagAsync(Guid userProxyId, Guid networkId, Guid customPageId, JsonElement pageBlockData);
    
}