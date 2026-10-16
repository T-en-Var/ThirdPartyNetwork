using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

// This service will directly access the other repositories.
// -> to prevent circular dependencies
// This will not return any values from the repository, only use that data to perform checks 
public class FeatureFlagValidator(
    INetworkRepository networkRepository,
    INetworkUserRepository networkUserRepository,
    IFileRepository fileRepository,
    IBlogRepository blogRepository,
    IConfigurationRepository configurationRepository,
    ICustomPageRepository customPageRepository) : IFeatureFlagValidator
{
    private async Task<Network> GetMainNetwork()
    {
        var network = await networkRepository.GetMainNetworkDetailsAsync();
        ValidateNetworkFeatureFlag(network);
        return network;
    }

    private async Task<Network> GetNetwork(Guid networkId)
    {
        var network = await networkRepository.GetByIdFeatureFlagAsync(networkId);
        ValidateNetworkFeatureFlag(network);

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
                .ThenInclude(n => n.FeatureFlag)
        );

        var networkUser = networkUsers.FirstOrDefault();
        ValidateNetworkUser(networkUser);
        return networkUser!;
    }

    public async Task<bool> CheckUserAddNetworkFileFeatureFlagAsync(Guid userProxyId, Guid networkId, IFormFile file)
    {
        // checks if:
        // - enough space in network
        // - enough space in user / role
        //

        // Network Checks
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowFiles) { return false; }
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowFiles) { return false; }

        // Network File Checks
        var networkFiles = await fileRepository.GetByNetworkIdAsync(networkId);
        if (!CheckNetworkFileCount(networkFiles.Count, network.FeatureFlag.FileCountLimit)) { return false; }
        if (!CheckNetworkFileSizeLimit(file, network.FeatureFlag.FileSizeLimit)) { return false; }
        if (!CheckNetworkFileStorageReached(networkFiles, file, network.FeatureFlag.FileStorageLimit)) { return false; }

        // User Checks. If a role limit exists, use the larger limit.
        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userFileSizeLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileSizeLimit)
            ?? network.FeatureFlag.FileSizeLimit;
        var userFiles = await fileRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userFileCountLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit)
            ?? network.FeatureFlag.FileCountLimit;
        var userFileStorageLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileStorageLimit)
            ?? network.FeatureFlag.FileStorageLimit;

        if (!CheckNetworkFileSizeLimit(file, userFileSizeLimit)) { return false; }
        if (!CheckNetworkFileCount(userFiles.Count, userFileCountLimit)) { return false; }
        if (!CheckNetworkFileStorageReached(userFiles, file, userFileStorageLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddBlogFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowBlogs) { return false; }
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowBlogs) { return false; }

        var networkBlogs = await blogRepository.GetByNetworkIdAsync(networkId);
        if (!CheckNetworkFileCount(networkBlogs.Count, network.FeatureFlag.BlogCountLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userBlogs = await blogRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userBlogCountLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.BlogCountLimit)
        ?? network.FeatureFlag.BlogCountLimit;

        if (!CheckNetworkFileCount(userBlogs.Count, userBlogCountLimit)) { return false; }

        return true;

    }

    public async Task<bool> CheckUserAddConfigurationFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {

        // Global Checks
        // Network Checks
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowConfigurations) { return false; }
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowConfigurations) { return false; }

        var networkConfigurations = await configurationRepository.GetByNetworkIdAsync(networkId);
        if (!CheckNetworkFileCount(networkConfigurations.Count, network.FeatureFlag.ConfigurationCountLimit)) { return false; }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userConfigs = await configurationRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userConfigurationCountLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.ConfigurationCountLimit)
        ?? network.FeatureFlag.ConfigurationCountLimit;

        if (!CheckNetworkFileCount(userConfigs.Count, userConfigurationCountLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {

        // Global Checks
        // Network Checks
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowCustomPages) { return false; }
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowCustomPages) { return false; }


        var networkCustomPages = await customPageRepository.GetByNetworkIdAsync(networkId);
        if (!CheckNetworkFileCount(networkCustomPages.Count, network.FeatureFlag.CustomPageCountLimit)) { return false; }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userCustomPageCountLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.CustomPageCountLimit) ?? network.FeatureFlag.CustomPageCountLimit;
        if (!CheckNetworkFileCount(userCustomPages.Count, userCustomPageCountLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageBlockFeatureFlagAsync(Guid userProxyId, Guid networkId,
        Guid customPageId, JsonElement pageBlockData)
    {
        var error = new Exception();
        var pageBlockCharCount = pageBlockData.GetRawText().Length;

        // Network Checks
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowCustomPages) { return false; }
        var networkCustomPage = await customPageRepository.GetByIdAsync(customPageId);
        if (!CheckNetworkFileCount(networkCustomPage?.Pages.Count ?? 0, network.FeatureFlag.CustomPageBlockCountLimit)) { return false; }
        if (!CheckNetworkFileCount(pageBlockCharCount, network.FeatureFlag.CustomPageBlockSizeLimit)) { return false; }

        // User Checks
        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userCustomPageBlockCountLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.CustomPageBlockCountLimit) ?? network.FeatureFlag.CustomPageBlockCountLimit;
        if (!CheckNetworkFileCount(userCustomPages.Count, userCustomPageBlockCountLimit)) { return false; }

        var userCustomPageBlockSizeLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.CustomPageBlockSizeLimit) ?? network.FeatureFlag.CustomPageBlockSizeLimit;
        if (!CheckNetworkFileCount(pageBlockCharCount, userCustomPageBlockSizeLimit)) { return false; }

        return true;
    }

    private void ValidateNetworkFeatureFlag(Network? network)
    {
        if (network == null)
        {
            throw new NotFoundHttpException("Network not found");
        }
    }

    private void ValidateNetworkUser(NetworkUser? networkUser)
    {
        if (networkUser == null)
        {
            throw new NotFoundHttpException("Networkuser not found");
        }
    }
    private bool CheckNetworkFileCount(int count, int fileCountLimit)
    {
        return count < fileCountLimit;
    }

    private bool CheckNetworkFileSizeLimit(IFormFile file, int fileSizeLimit)
    {
        return file.Length <= fileSizeLimit * 1024.0; // FileSizeLimit is in KB
    }

    private bool CheckNetworkFileStorageReached(ICollection<thirdpartynetwork.Core.Models.File> files, IFormFile file, int fileStorageLimit)
    {
        var totalNetworkFileStorageSize = files.Sum(networkFile => networkFile.SizeBytes);
        return totalNetworkFileStorageSize + file.Length <= fileStorageLimit * 1024.0; // FileStorageLimit is in KB the limit
    }
}