using System.Text;
using System.Text.Json;
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
    ICustomPageRepository customPageRepository,
    INetworkEntitlementValidator networkEntitlementValidator) : IEntitlementService
{
    private async Task<Network> GetMainNetwork()
    {
        var network = await networkRepository.GetMainNetworkDetailsAsync();
        networkEntitlementValidator.ValidateNetworkEntitlement(network);
        return network;
    }

    private async Task<Network> GetNetwork(Guid networkId)
    {
        var network = await networkRepository.GetByIdEntitlementAsync(networkId);
        networkEntitlementValidator.ValidateNetworkEntitlement(network);

        return network!;
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
        networkEntitlementValidator.ValidateNetworkUser(networkUser);
        return networkUser!;
    }

    public async Task<bool> CheckUserAddNetworkFileEntitlementAsync(Guid userProxyId, Guid networkId, IFormFile file)
    {
        // checks if:
        // - enough space in network
        // - enough space in user / role
        //

        // Network Checks
        try
        {
            var mainNetwork = await GetMainNetwork();
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(mainNetwork.Entitlement.AllowFiles);
            var network = await GetNetwork(networkId);
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(network.Entitlement.AllowFiles);

            // Network File Checks
            var networkFiles = await fileRepository.GetByNetworkIdAsync(networkId);
            networkEntitlementValidator.CheckNetworkFileCount(networkFiles.Count, network.Entitlement.FileCountLimit);
            networkEntitlementValidator.CheckNetworkFileSizeLimit(file, network.Entitlement.FileSizeLimit);
            networkEntitlementValidator.CheckNetworkFileStorageReached(networkFiles, file, network.Entitlement.FileStorageLimit);

            // User Checks. If a role limit exists, use the larger limit.
            var networkUser = await GetNetworkUser(userProxyId, networkId);
            var userFileSizeLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.FileSizeLimit) 
                ?? network.Entitlement.FileSizeLimit;
            var userFiles = await fileRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
            var userFileCountLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.FileCountLimit) 
                ?? network.Entitlement.FileCountLimit;
            var userFileStorageLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.FileStorageLimit) 
                ?? network.Entitlement.FileStorageLimit;
                
            networkEntitlementValidator.CheckNetworkFileSizeLimit(file, userFileSizeLimit);
            networkEntitlementValidator.CheckNetworkFileCount(userFiles.Count, userFileCountLimit);
            networkEntitlementValidator.CheckNetworkFileStorageReached(userFiles, file, userFileStorageLimit);

        return true; 

        }
        catch (BaseHttpException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(new StringBuilder().Append("mainNetwork.Entitlement.AllowFiles").Append(ex.Message).ToString());
            return false;
        }
    }

    public async Task<bool> CheckUserAddBlogEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        try
        {
            var mainNetwork = await GetMainNetwork();
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(mainNetwork.Entitlement.AllowBlogs);
            var network = await GetNetwork(networkId);
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(network.Entitlement.AllowBlogs);

            var networkBlogs = await blogRepository.GetByNetworkIdAsync(networkId);
            networkEntitlementValidator.CheckNetworkFileCount(networkBlogs.Count, network.Entitlement.BlogCountLimit);

            var networkUser = await GetNetworkUser(userProxyId, networkId);
            var userBlogs = await blogRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
            var userBlogCountLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.BlogCountLimit) 
            ?? network.Entitlement.BlogCountLimit;

            networkEntitlementValidator.CheckNetworkFileCount(userBlogs.Count, userBlogCountLimit);

            return true;

        }
        catch (BaseHttpException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(new StringBuilder().Append("mainNetwork.Entitlement.AllowBlogs").Append(ex.Message).ToString());
            return false;
        }
    }

    public async Task<bool> CheckUserAddConfigurationEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        try
        {
            // Global Checks
            // Network Checks
            var mainNetwork = await GetMainNetwork();
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(mainNetwork.Entitlement.AllowConfigurations);
            var network = await GetNetwork(networkId);
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(network.Entitlement.AllowConfigurations);

            var networkConfigurations = await configurationRepository.GetByNetworkIdAsync(networkId);
            networkEntitlementValidator.CheckNetworkFileCount(networkConfigurations.Count, network.Entitlement.ConfigurationCountLimit);

            // User Checks
            var networkUser = await GetNetworkUser(userProxyId, networkId);
            var userConfigs = await configurationRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
            var userConfigurationCountLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.ConfigurationCountLimit) 
            ?? network.Entitlement.ConfigurationCountLimit;
            
            networkEntitlementValidator.CheckNetworkFileCount(userConfigs.Count, userConfigurationCountLimit);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(new StringBuilder().Append("mainNetwork.Entitlement.AllowConfigurations").Append(ex.Message).ToString());
            return false;
        }
    }

    public async Task<bool> CheckUserAddCustomPageEntitlementAsync(Guid userProxyId, Guid networkId)
    {
        try
        {
            // Global Checks
            // Network Checks
            var mainNetwork = await GetMainNetwork();
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(mainNetwork.Entitlement.AllowCustomPages);
            var network = await GetNetwork(networkId);
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(network.Entitlement.AllowCustomPages);


            var networkCustomPages = await customPageRepository.GetByNetworkIdAsync(networkId);
            networkEntitlementValidator.CheckNetworkFileCount(networkCustomPages.Count, network.Entitlement.CustomPageCountLimit);

            // User Checks
            var networkUser = await GetNetworkUser(userProxyId, networkId);
            var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
            var userCustomPageCountLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.CustomPageCountLimit) ?? network.Entitlement.CustomPageCountLimit;
            networkEntitlementValidator.CheckNetworkFileCount(userCustomPages.Count, userCustomPageCountLimit);

            return true;   
        }
        catch (Exception ex)
        {
            Console.WriteLine(new StringBuilder().Append("mainNetwork.Entitlement.AllowCustomPages").Append(ex.Message).ToString());
            return false;
        }
    }

    public async Task<bool> CheckUserAddCustomPageBlockEntitlementAsync(Guid userProxyId, Guid networkId,
        Guid customPageId, JsonElement pageBlockData)
    {
        try
        {
            var error = new Exception();
            var pageBlockCharCount = pageBlockData.GetRawText().Length;

            // Network Checks
            var network = await GetNetwork(networkId);
            networkEntitlementValidator.CheckNetworkPropertyIsAllowed(network.Entitlement.AllowCustomPages);
            var networkCustomPage = await customPageRepository.GetByIdAsync(customPageId);
            networkEntitlementValidator.CheckNetworkFileCount(networkCustomPage?.Pages.Count ?? 0, network.Entitlement.CustomPageBlockCountLimit);
            networkEntitlementValidator.CheckNetworkFileCount(pageBlockCharCount, network.Entitlement.CustomPageBlockSizeLimit);

            // User Checks
            var networkUser = await GetNetworkUser(userProxyId, networkId);
            var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
            var userCustomPageBlockCountLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.CustomPageBlockCountLimit) ?? network.Entitlement.CustomPageBlockCountLimit;
            networkEntitlementValidator.CheckNetworkFileCount(userCustomPages.Count, userCustomPageBlockCountLimit);

            var userCustomPageBlockSizeLimit = NetworkEntitlementWriterFactory.ReadEntitlement<int?>(networkUser, NetworkEntitlementWriterFactory.EntitlementWriterType.CustomPageBlockSizeLimit) ?? network.Entitlement.CustomPageBlockSizeLimit;
            networkEntitlementValidator.CheckNetworkFileCount(pageBlockCharCount, userCustomPageBlockSizeLimit);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(new StringBuilder().Append("mainNetwork.Entitlement.AllowCustomPages").Append(ex.Message).ToString());
            return false;
        }
    }
}