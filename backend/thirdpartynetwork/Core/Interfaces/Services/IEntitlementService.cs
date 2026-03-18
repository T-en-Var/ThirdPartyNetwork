using System.Text.Json;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IEntitlementService
{
    Task<bool> CheckUserAddNetworkFileEntitlementAsync(Guid userProxyId, Guid networkId, IFormFile file);
    Task<bool> CheckUserAddBlogEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddConfigurationEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageEntitlementAsync(Guid userProxyId, Guid networkId);
    Task<bool> CheckUserAddCustomPageBlockEntitlementAsync(Guid userProxyId, Guid networkId, Guid customPageId, JsonElement pageBlockData);
    
    NetworkUser WriteCustomPageBlockCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit);
    int? ReadCustomPageBlockCountLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveCustomPageBlockCountLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser, int blogCountLimit);
    int? ReadCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteCustomPageCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit);
    int? ReadCustomPageCountLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveCustomPageCountLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteConfigurationCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit);
    int? ReadConfigurationCountLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveConfigurationCountLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteBlogCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit);
    int? ReadBlogCountLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveBlogCountLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteFileCountLimitEntitlement(NetworkUser networkUser, int fileCountLimit);
    int? ReadFileCountLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveFileCountLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteFileSizeLimitEntitlement(NetworkUser networkUser, int fileCountLimit);
    int? ReadFileSizeLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveFileSizeLimitEntitlement(NetworkUser networkUser);
    
    NetworkUser WriteFileStorageLimitEntitlement(NetworkUser networkUser, int fileCountLimit);
    int? ReadFileStorageLimitEntitlement(NetworkUser networkUser);
    NetworkUser RemoveFileStorageLimitEntitlement(NetworkUser networkUser);
}