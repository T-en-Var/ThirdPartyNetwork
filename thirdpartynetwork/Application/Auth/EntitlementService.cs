using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

// This service will directly access the other repositories.
// -> to prevent circular dependencies
// This will not return any values from the repository, only use that data to perform checks 
public class EntitlementService(
    INetworkRepository networkRepository,
    INetworkUserRepository networkUserRepository,
    IFileRepository fileRepository,
    IBlogRepository blogRepository,
    IConfigurationRepository configurationRepository,
    ICustomPageRepository customPageRepository) : IEntitlementService
{
    private async Task<Network> GetMainNetwork()
    {
        var network = await networkRepository.GetMainNetworkDetailsAsync();
        return network;
    }
    
    private async Task<Network> GetNetwork(Guid networkId)
    {
        var network = await networkRepository.GetByIdEntitlementAsync(networkId);

        if (network == null)
        {
            throw new NotFoundHttpException("Network not found");
        }

        return network;
    }

    private async Task<NetworkUser> GetNetworkUser(Guid userProxyId, Guid networkId)
    {
        var networkUsers = await networkUserRepository.GetByUserProxyIdAsync(
            userProxyId,
            q => q
                .Where(nu => nu.NetworkId == networkId)
                .Include(u => u.NetworkUserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Network)
                .ThenInclude(n => n.Entitlement)
        );

        var networkUser = networkUsers.FirstOrDefault();

        if (networkUser == null)
        {
            throw new NotFoundHttpException("NetworkUser not found");
        }

        return networkUser;
    }

    public async Task<bool> CheckUserAddNetworkFileEntitlementAsync(Guid userProxyId, Guid networkId, IFormFile file)
    {
        // checks if:
        // - enough space in network
        // - enough space in user / role
        //
        
        // Global Checks
        var mainNetwork = await GetMainNetwork();

        if (!mainNetwork.Entitlement.AllowFiles)
        {
            Console.WriteLine("mainNetwork.Entitlement.AllowFiles was false");
            return false;
        }

        // Network Checks
        var network = await GetNetwork(networkId);

        if (!network.Entitlement.AllowFiles)
        {
            Console.WriteLine("network.Entitlement.AllowFiles was false");
            return false;
        }

        var networkFiles = await fileRepository.GetByNetworkIdAsync(networkId);
        if (networkFiles.Count >= network.Entitlement.FileCountLimit)
        {
            Console.WriteLine("network.Entitlement.FileCountLimit reached");
            return false;
        }

        if (file.Length > network.Entitlement.FileSizeLimit * 1024.0) // FileSizeLimit is in KB
        {
            Console.WriteLine("network.Entitlement.FileSizeLimit reached");
            return false;
        }

        var totalNetworkFileStorageSize = networkFiles.Sum(networkFile => networkFile.SizeBytes);
        if (totalNetworkFileStorageSize + file.Length >
            network.Entitlement.FileStorageLimit * 1024.0) // FileStorageLimit is in KB
        {
            Console.WriteLine("network.Entitlement.FileStorageLimit reached");
            return false;
        }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);

        // Includes role limit, if found takes the largest value.
        var userFileSizeLimit = ReadFileSizeLimitEntitlement(networkUser) ?? network.Entitlement.FileSizeLimit;
        if (file.Length > userFileSizeLimit * 1024.0)
        {
            Console.WriteLine("userFileSizeLimit reached");
            return false;
        }

        var userFiles = await fileRepository.GetByAuthorIdAsync(networkId, networkUser.Id);

        var userFileCountLimit = ReadFileCountLimitEntitlement(networkUser) ?? network.Entitlement.FileCountLimit;
        if (userFiles.Count >= userFileCountLimit)
        {
            Console.WriteLine("userFileCountLimit reached");
            return false;
        }

        var totalUserFileStorageSize = userFiles.Sum(networkFile => networkFile.SizeBytes);
        var userFileStorageLimit =
            ReadFileStorageLimitEntitlement(networkUser) ?? network.Entitlement.FileStorageLimit; // is in KB
        if (totalUserFileStorageSize + file.Length > userFileStorageLimit * 1024.0)
        {
            Console.WriteLine("totalUserFileStorageSize reached");
            return false;
        }

        return true;
    }

    public async Task<bool> CheckUserAddBlogEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        // Global Checks
        var mainNetwork = await GetMainNetwork();

        if (!mainNetwork.Entitlement.AllowBlogs)
        {
            Console.WriteLine("mainNetwork.Entitlement.AllowBlogs was false");
            return false;
        }
        
        // Network Checks
        var network = await GetNetwork(networkId);

        if (!network.Entitlement.AllowBlogs)
        {
            Console.WriteLine("network.Entitlement.AllowBlogs was false");
            return false;
        }

        var networkBlogs = await blogRepository.GetByNetworkIdAsync(networkId);
        if (networkBlogs.Count >= network.Entitlement.BlogCountLimit)
        {
            Console.WriteLine("network.Entitlement.BlogCountLimit reached");
            return false;
        }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);

        var userBlogs = await blogRepository.GetByAuthorIdAsync(networkId, networkUser.Id);

        var userBlogCountLimit = ReadBlogCountLimitEntitlement(networkUser) ?? network.Entitlement.BlogCountLimit;
        if (userBlogs.Count >= userBlogCountLimit)
        {
            Console.WriteLine("userBlogCountLimit reached");
            return false;
        }

        return true;
    }

    public async Task<bool> CheckUserAddConfigurationEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        // Global Checks
        var mainNetwork = await GetMainNetwork();

        if (!mainNetwork.Entitlement.AllowConfigurations)
        {
            Console.WriteLine("mainNetwork.Entitlement.AllowConfigurations was false");
            return false;
        }

        // Network Checks
        var network = await GetNetwork(networkId);

        if (!network.Entitlement.AllowConfigurations)
        {
            Console.WriteLine("network.Entitlement.AllowConfigurations was false");
            return false;
        }

        var networkConfigurations = await configurationRepository.GetByNetworkIdAsync(networkId);
        if (networkConfigurations.Count >= network.Entitlement.ConfigurationCountLimit)
        {
            Console.WriteLine("network.Entitlement.ConfigurationCountLimit reached");
            return false;
        }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);

        var userConfigs = await configurationRepository.GetByAuthorIdAsync(networkId, networkUser.Id);

        var userConfigurationCountLimit = ReadConfigurationCountLimitEntitlement(networkUser) ??
                                          network.Entitlement.ConfigurationCountLimit;
        if (userConfigs.Count >= userConfigurationCountLimit)
        {
            Console.WriteLine("userConfigurationCountLimit reached");
            return false;
        }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        // Global Checks
        var mainNetwork = await GetMainNetwork();

        if (!mainNetwork.Entitlement.AllowCustomPages)
        {
            Console.WriteLine("mainNetwork.Entitlement.AllowCustomPages was false");
            return false;
        }

        // Network Checks
        var network = await GetNetwork(networkId);

        if (!network.Entitlement.AllowCustomPages)
        {
            Console.WriteLine("network.Entitlement.AllowCustomPages was false");
            return false;
        }

        var networkCustomPages = await customPageRepository.GetByNetworkIdAsync(networkId);
        if (networkCustomPages.Count >= network.Entitlement.CustomPageCountLimit)
        {
            Console.WriteLine("network.Entitlement.CustomPageCountLimit reached");
            return false;
        }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);

        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);

        var userCustomPageCountLimit =
            ReadCustomPageCountLimitEntitlement(networkUser) ?? network.Entitlement.CustomPageCountLimit;
        if (userCustomPages.Count >= userCustomPageCountLimit)
        {
            Console.WriteLine("userCustomPageCountLimit reached");
            return false;
        }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageBlockEntitlementAsync(Guid userProxyId, Guid networkId,
        Guid customPageId, JsonElement pageBlockData)
    {
        var pageBlockCharCount = pageBlockData.GetRawText().Length;
        
        // Network Checks
        var network = await GetNetwork(networkId);

        if (!network.Entitlement.AllowCustomPages)
        {
            Console.WriteLine("network.Entitlement.AllowCustomPages was false");
            return false;
        }

        var networkCustomPage = await customPageRepository.GetByIdAsync(customPageId);
        if ((networkCustomPage?.Pages.Count ?? 0) >= network.Entitlement.CustomPageBlockCountLimit)
        {
            Console.WriteLine("network.Entitlement.CustomPageBlockCountLimit reached");
            return false;
        }
        
        if (pageBlockCharCount >= network.Entitlement.CustomPageBlockSizeLimit)
        {
            Console.WriteLine("userCustomPageBlockSizeLimit reached");
            return false;
        }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);

        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);

        var userCustomPageBlockCountLimit = ReadCustomPageBlockCountLimitEntitlement(networkUser) ??
                                            network.Entitlement.CustomPageBlockCountLimit;
        if (userCustomPages.Count >= userCustomPageBlockCountLimit)
        {
            Console.WriteLine("userCustomPageBlockCountLimit reached");
            return false;
        }

        var userCustomPageBlockSizeLimit = ReadCustomPageBlockSizeLimitEntitlement(networkUser) ??
                                           network.Entitlement.CustomPageBlockSizeLimit;
        if (pageBlockCharCount >= userCustomPageBlockSizeLimit)
        {
            Console.WriteLine("userCustomPageBlockSizeLimit reached");
            return false;
        }

        return true;
    }

    private static NetworkUser WriteEntitlement<T>(NetworkUser networkUser, string key, T value)
    {
        var jsonString = networkUser.Entitlements.RootElement.GetRawText();
        var root = JsonNode.Parse(jsonString) ?? new JsonObject();

        root[key] = JsonValue.Create(value);
        networkUser.Entitlements = JsonDocument.Parse(root.ToJsonString());

        return networkUser;
    }

    private static T? ReadEntitlement<T>(JsonDocument? entitlements, string key, T? defaultValue = default)
    {
        if (entitlements == null) return defaultValue;
        if (entitlements.RootElement.TryGetProperty(key, out var element))
        {
            return element.Deserialize<T>() ?? defaultValue;
        }

        return defaultValue;
    }

    private static NetworkUser RemoveEntitlement(NetworkUser networkUser, string key)
    {
        var root = JsonNode.Parse(networkUser.Entitlements.RootElement.GetRawText())?.AsObject();

        if (root != null && root.Remove(key))
        {
            // Only update the property if something was actually removed
            networkUser.Entitlements = JsonDocument.Parse(root.ToJsonString());
        }

        return networkUser;
    }

    // CustomPage PageBlock Size Limit
    private const string CustomPageBlockSizeLimit = "CustomPageBlockSizeLimit";

    public NetworkUser WriteCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser, int blogCountLimit)
    {
        return WriteEntitlement(networkUser, CustomPageBlockSizeLimit, blogCountLimit);
    }

    public int? ReadCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, CustomPageBlockSizeLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, CustomPageBlockSizeLimit))
            .Max();
    }

    public NetworkUser RemoveCustomPageBlockSizeLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, CustomPageBlockSizeLimit);
    }

    // CustomPage PageBlock Count Limit
    private const string CustomPageBlockCountLimit = "CustomPageBlockCountLimit";

    public NetworkUser WriteCustomPageBlockCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit)
    {
        return WriteEntitlement(networkUser, CustomPageBlockCountLimit, blogCountLimit);
    }

    public int? ReadCustomPageBlockCountLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, CustomPageBlockCountLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, CustomPageBlockCountLimit))
            .Max();
    }

    public NetworkUser RemoveCustomPageBlockCountLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, CustomPageBlockCountLimit);
    }

    // CustomPage Count Limit
    private const string CustomPageCountLimit = "CustomPageCountLimit";

    public NetworkUser WriteCustomPageCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit)
    {
        return WriteEntitlement(networkUser, CustomPageCountLimit, blogCountLimit);
    }

    public int? ReadCustomPageCountLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, CustomPageCountLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, CustomPageCountLimit))
            .Max();
    }

    public NetworkUser RemoveCustomPageCountLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, CustomPageCountLimit);
    }

    // Configuration Count Limit
    private const string ConfigurationCountLimit = "ConfigurationCountLimit";

    public NetworkUser WriteConfigurationCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit)
    {
        return WriteEntitlement(networkUser, ConfigurationCountLimit, blogCountLimit);
    }

    public int? ReadConfigurationCountLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, ConfigurationCountLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, ConfigurationCountLimit))
            .Max();
    }

    public NetworkUser RemoveConfigurationCountLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, ConfigurationCountLimit);
    }

    // Blog Count Limit
    private const string BlogCountLimit = "BlogCountLimit";

    public NetworkUser WriteBlogCountLimitEntitlement(NetworkUser networkUser, int blogCountLimit)
    {
        return WriteEntitlement(networkUser, BlogCountLimit, blogCountLimit);
    }

    public int? ReadBlogCountLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, BlogCountLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, BlogCountLimit))
            .Max();
    }

    public NetworkUser RemoveBlogCountLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, BlogCountLimit);
    }

    // File Count Limit
    private const string FileCountLimit = "FileCountLimit";

    public NetworkUser WriteFileCountLimitEntitlement(NetworkUser networkUser, int fileCountLimit)
    {
        return WriteEntitlement(networkUser, FileCountLimit, fileCountLimit);
    }

    public int? ReadFileCountLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, FileCountLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, FileCountLimit))
            .Max();
    }

    public NetworkUser RemoveFileCountLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, FileCountLimit);
    }

    // File Size Limit
    private const string FileSizeLimit = "FileSizeLimit";

    public NetworkUser WriteFileSizeLimitEntitlement(NetworkUser networkUser, int fileCountLimit)
    {
        return WriteEntitlement(networkUser, FileSizeLimit, fileCountLimit);
    }

    public int? ReadFileSizeLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, FileSizeLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, FileSizeLimit))
            .Max();
    }

    public NetworkUser RemoveFileSizeLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, FileSizeLimit);
    }

    // File Storage Limit
    private const string FileStorageLimit = "FileStorageLimit";

    public NetworkUser WriteFileStorageLimitEntitlement(NetworkUser networkUser, int fileCountLimit)
    {
        return WriteEntitlement(networkUser, FileStorageLimit, fileCountLimit);
    }

    public int? ReadFileStorageLimitEntitlement(NetworkUser networkUser)
    {
        var onUser = ReadEntitlement<int?>(networkUser.Entitlements, FileStorageLimit);
        if (onUser != null) return onUser;
        
        return networkUser.NetworkUserRoles
            .Select(x => ReadEntitlement<int?>(x.Role.Entitlements, FileStorageLimit))
            .Max();
    }

    public NetworkUser RemoveFileStorageLimitEntitlement(NetworkUser networkUser)
    {
        return RemoveEntitlement(networkUser, FileStorageLimit);
    }
}