using System.Threading.Tasks.Dataflow;
using thirdpartynetwork.Core.Models;

public static class NetworkEntitlementWriterFactory
{
    public enum EntitlementWriterType
    {
        BlogCountLimit,
        ConfigurationCountLimit,
        FileSizeLimit,
        CustomPageCountLimit,
        CustomPageBlockCountLimit,
        FileCountLimit,
        FileStorageLimit,
        CustomPageBlockSizeLimit
    }
    public static NetworkUser WriteEntitlement<T>(NetworkUser networkUser, T value, EntitlementWriterType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        return NetworkEntitlementWriter.WriteEntitlement(networkUser, key, value);
    }

    private static string GetEntitlementKey(EntitlementWriterType entitlementWriterType)
    {
        return entitlementWriterType switch
        {
            EntitlementWriterType.BlogCountLimit => "BlogCountLimit",
            EntitlementWriterType.ConfigurationCountLimit => "ConfigurationCountLimit",
            EntitlementWriterType.CustomPageCountLimit => "CustomPageCountLimit",
            EntitlementWriterType.CustomPageBlockCountLimit => "CustomPageBlockCountLimit",
            EntitlementWriterType.CustomPageBlockSizeLimit => "CustomPageBlockSizeLimit",
            EntitlementWriterType.FileSizeLimit => "FileSizeLimit",
            EntitlementWriterType.FileCountLimit => "FileCountLimit",
            EntitlementWriterType.FileStorageLimit => "FileStorageLimit",
            _ => throw new ArgumentOutOfRangeException(nameof(entitlementWriterType), entitlementWriterType, null)
        };
    }

    public static NetworkUser RemoveEntitlement<T>(NetworkUser networkUser, EntitlementWriterType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        return NetworkEntitlementWriter.RemoveEntitlement(networkUser, key);
    }

    public static int? ReadEntitlement<T>(NetworkUser networkUser, EntitlementWriterType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        var limits = new List<int?>
        {
            NetworkEntitlementWriter.ReadEntitlement<int?>(networkUser.Entitlements, key)
        };

        if (networkUser.NetworkUserRoles != null)
        {
            limits.AddRange(networkUser.NetworkUserRoles
                .Select(x => NetworkEntitlementWriter.ReadEntitlement<int?>(x.Role.Entitlements, key)));
        }

        return limits.Max();
    }
    
}