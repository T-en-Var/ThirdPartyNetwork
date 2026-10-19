using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

public static class FeatureFlagHelper
{   
    public static bool CheckLimitCount(int count, int fileCountLimit)
    {
        return count < fileCountLimit;
    }

    public static bool CheckFileSizeLimit(IFormFile file, int fileSizeLimit)
    {
        return file.Length <= fileSizeLimit * 1024.0; // FileSizeLimit is in KB
    }

    public static bool CheckNetworkFileStorageReached(ICollection<thirdpartynetwork.Core.Models.File> files, IFormFile file, int fileStorageLimit)
    {
        var totalNetworkFileStorageSize = files.Sum(networkFile => networkFile.SizeBytes);
        return totalNetworkFileStorageSize + file.Length <= fileStorageLimit * 1024.0; // FileStorageLimit is in KB the limit
    }

    public static int? GetUserFeatureFlagAndRoleFeatureFlag(NetworkUser networkUser, FeatureFlagType featureFlagType)
    {
        int? userFeatureFlag = null;
        switch (featureFlagType)
        {
            case FeatureFlagType.FileCountLimit:
                userFeatureFlag = networkUser.FeatureFlags.FileCountLimit;
                break;
            case FeatureFlagType.FileSizeLimit:
                userFeatureFlag = networkUser.FeatureFlags.FileSizeLimit;
                break;
            case FeatureFlagType.FileStorageLimit:
                userFeatureFlag = networkUser.FeatureFlags.FileStorageLimit;
                break;
            case FeatureFlagType.BlogCountLimit:
                userFeatureFlag = networkUser.FeatureFlags.BlogCountLimit;
                break;
            case FeatureFlagType.ConfigurationCountLimit:
                userFeatureFlag = networkUser.FeatureFlags.ConfigurationCountLimit;
                break;
            case FeatureFlagType.CustomPageCountLimit:
                userFeatureFlag = networkUser.FeatureFlags.CustomPageCountLimit;
                break;
            case FeatureFlagType.CustomPageBlockCountLimit:
                userFeatureFlag = networkUser.FeatureFlags.CustomPageBlockCountLimit;
                break;
            case FeatureFlagType.CustomPageBlockSizeLimit:
                userFeatureFlag = networkUser.FeatureFlags.CustomPageBlockSizeLimit;
                break;
        }
        var roleFeatureFlag = GetRoleFeatureFlag(networkUser.NetworkUserRoles, featureFlagType);
        return userFeatureFlag ?? roleFeatureFlag;
    }

    public static int? GetRoleFeatureFlag(ICollection<NetworkUserRole> networkUserRoles, FeatureFlagType featureFlagType)
    {
        switch (featureFlagType)
        {
            case FeatureFlagType.FileCountLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.FileCountLimit);
            case FeatureFlagType.FileSizeLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.FileSizeLimit);
            case FeatureFlagType.FileStorageLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.FileStorageLimit);
            case FeatureFlagType.BlogCountLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.BlogCountLimit);
            case FeatureFlagType.ConfigurationCountLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.ConfigurationCountLimit);
            case FeatureFlagType.CustomPageCountLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.CustomPageCountLimit);
            case FeatureFlagType.CustomPageBlockCountLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.CustomPageBlockCountLimit);
            case FeatureFlagType.CustomPageBlockSizeLimit:
                return networkUserRoles.Max(role => role.Role.FeatureFlags.CustomPageBlockSizeLimit);
            default:
                return null;
        }
    }
}