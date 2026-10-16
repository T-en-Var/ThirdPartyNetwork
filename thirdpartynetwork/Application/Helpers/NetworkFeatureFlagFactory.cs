using System.Threading.Tasks.Dataflow;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

public static class NetworkFeatureFlagFactory
{
    public static NetworkUser WriteFeatureFlag<T>(NetworkUser networkUser, T value, FeatureFlagType featureFlagWriterType)
    {
        var key = GetFeatureFlagKey(featureFlagWriterType);
        return NetworkFeatureFlagHelper.WriteFeatureFlag(networkUser, key, value);
    }

    public static NetworkUser RemoveFeatureFlag<T>(NetworkUser networkUser, FeatureFlagType featureFlagWriterType)
    {
        var key = GetFeatureFlagKey(featureFlagWriterType);
        return NetworkFeatureFlagHelper.RemoveFeatureFlag(networkUser, key);
    }

    public static int? ReadFeatureFlag<T>(NetworkUser networkUser, FeatureFlagType featureFlagWriterType)
    {
        var key = GetFeatureFlagKey(featureFlagWriterType);
        var limits = new List<int?>
        {
            NetworkFeatureFlagHelper.ReadFeatureFlag<int?>(networkUser.FeatureFlags, key)
        };

        if (networkUser.NetworkUserRoles != null)
        {
            limits.AddRange(networkUser.NetworkUserRoles
                .Select(x => NetworkFeatureFlagHelper.ReadFeatureFlag<int?>(x.Role.FeatureFlags, key)));
        }

        return limits.Max();
    }
    
    private static string GetFeatureFlagKey(FeatureFlagType featureFlagWriterType)
    {
        return featureFlagWriterType switch
        {
            FeatureFlagType.BlogCountLimit => "BlogCountLimit",
            FeatureFlagType.ConfigurationCountLimit => "ConfigurationCountLimit",
            FeatureFlagType.CustomPageCountLimit => "CustomPageCountLimit",
            FeatureFlagType.CustomPageBlockCountLimit => "CustomPageBlockCountLimit",
            FeatureFlagType.CustomPageBlockSizeLimit => "CustomPageBlockSizeLimit",
            FeatureFlagType.FileSizeLimit => "FileSizeLimit",
            FeatureFlagType.FileCountLimit => "FileCountLimit",
            FeatureFlagType.FileStorageLimit => "FileStorageLimit",
            _ => throw new ArgumentOutOfRangeException(nameof(featureFlagWriterType), featureFlagWriterType, null)
        };
    }

}