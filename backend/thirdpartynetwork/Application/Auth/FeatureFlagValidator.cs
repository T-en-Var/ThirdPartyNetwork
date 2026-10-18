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
        return await networkRepository.GetMainNetworkDetailsAsync() ?? throw new NotFoundHttpException("Network not found");
    }

    private async Task<Network> GetNetwork(Guid networkId)
    {
        return await networkRepository.GetByIdFeatureFlagAsync(networkId) ?? throw new NotFoundHttpException("Network not found");
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

        return networkUsers.FirstOrDefault() ?? throw new NotFoundHttpException("Networkuser not found");
    }

    public async Task<bool> CheckUserAddNetworkFileFeatureFlagAsync(Guid userProxyId, Guid networkId, IFormFile file)
    {
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowFiles) { return false; }

        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowFiles) { return false; }

        var networkFiles = await fileRepository.GetByNetworkIdAsync(networkId);
        if (!FeatureFlagHelper.CheckLimitCount(networkFiles.Count, network.FeatureFlag.FileCountLimit)) { return false; }
        if (!FeatureFlagHelper.CheckFileSizeLimit(file, network.FeatureFlag.FileSizeLimit)) { return false; }
        if (!FeatureFlagHelper.CheckNetworkFileStorageReached(networkFiles, file, network.FeatureFlag.FileStorageLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userFileSizeLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.FileSizeLimit) ?? network.FeatureFlag.FileSizeLimit;
        var userFiles = await fileRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userFileCountLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.FileCountLimit) ?? network.FeatureFlag.FileCountLimit;
        var userFileStorageLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.FileStorageLimit) ?? network.FeatureFlag.FileStorageLimit;
        if (!FeatureFlagHelper.CheckFileSizeLimit(file, userFileSizeLimit)) { return false; }
        if (!FeatureFlagHelper.CheckLimitCount(userFiles.Count, userFileCountLimit)) { return false; }
        if (!FeatureFlagHelper.CheckNetworkFileStorageReached(userFiles, file, userFileStorageLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddBlogFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowBlogs) { return false; }

        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowBlogs) { return false; }

        var networkBlogs = await blogRepository.GetByNetworkIdAsync(networkId);
        if (!FeatureFlagHelper.CheckLimitCount(networkBlogs.Count, network.FeatureFlag.BlogCountLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userBlogCountLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.BlogCountLimit) ?? network.FeatureFlag.BlogCountLimit;
        var userBlogs = await blogRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        if (!FeatureFlagHelper.CheckLimitCount(userBlogs.Count, userBlogCountLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddConfigurationFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowConfigurations) { return false; }

        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowConfigurations) { return false; }

        var networkConfigurations = await configurationRepository.GetByNetworkIdAsync(networkId);
        if (!FeatureFlagHelper.CheckLimitCount(networkConfigurations.Count, network.FeatureFlag.ConfigurationCountLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userConfigs = await configurationRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userConfigurationCountLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.ConfigurationCountLimit) ?? network.FeatureFlag.ConfigurationCountLimit;
        if (!FeatureFlagHelper.CheckLimitCount(userConfigs.Count, userConfigurationCountLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageFeatureFlagAsync(Guid userProxyId, Guid networkId)
    {
        var mainNetwork = await GetMainNetwork();
        if (!mainNetwork.FeatureFlag.AllowCustomPages) { return false; }

        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowCustomPages) { return false; }

        var networkCustomPages = await customPageRepository.GetByNetworkIdAsync(networkId);
        if (!FeatureFlagHelper.CheckLimitCount(networkCustomPages.Count, network.FeatureFlag.CustomPageCountLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userCustomPageCountLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.CustomPageCountLimit) ?? network.FeatureFlag.CustomPageCountLimit;
        if (!FeatureFlagHelper.CheckLimitCount(userCustomPages.Count, userCustomPageCountLimit)) { return false; }

        return true;
    }

    public async Task<bool> CheckUserAddCustomPageBlockFeatureFlagAsync(Guid userProxyId, Guid networkId,
        Guid customPageId, JsonElement pageBlockData)
    {
        var pageBlockCharCount = pageBlockData.GetRawText().Length;
        var network = await GetNetwork(networkId);
        if (!network.FeatureFlag.AllowCustomPages) { return false; }

        var networkCustomPage = await customPageRepository.GetByIdAsync(customPageId);
        if (!FeatureFlagHelper.CheckLimitCount(networkCustomPage?.Pages.Count ?? 0, network.FeatureFlag.CustomPageBlockCountLimit)) { return false; }
        if (!FeatureFlagHelper.CheckLimitCount(pageBlockCharCount, network.FeatureFlag.CustomPageBlockSizeLimit)) { return false; }

        var networkUser = await GetNetworkUser(userProxyId, networkId);
        var userCustomPages = await customPageRepository.GetByAuthorIdAsync(networkId, networkUser.Id);
        var userCustomPageBlockCountLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.CustomPageBlockCountLimit) ?? network.FeatureFlag.CustomPageBlockCountLimit;
        if (!FeatureFlagHelper.CheckLimitCount(userCustomPages.Count, userCustomPageBlockCountLimit)) { return false; }

        var userCustomPageBlockSizeLimit = FeatureFlagHelper.GetUserFeatureFlagAndRoleFeatureFlag(networkUser, FeatureFlagType.CustomPageBlockSizeLimit) ?? network.FeatureFlag.CustomPageBlockSizeLimit;
        if (!FeatureFlagHelper.CheckLimitCount(pageBlockCharCount, userCustomPageBlockSizeLimit)) { return false; }

        return true;
    }
}