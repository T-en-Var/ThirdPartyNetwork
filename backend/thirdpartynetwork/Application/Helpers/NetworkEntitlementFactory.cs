using System.Threading.Tasks.Dataflow;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

public static class NetworkEntitlementFactory
{
    public static NetworkUser WriteEntitlement<T>(NetworkUser networkUser, T value, EntitlementType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);
        return NetworkEntitlementHelper.WriteEntitlement(networkUser, key, value);
    }

    public static NetworkUser RemoveEntitlement<T>(NetworkUser networkUser, EntitlementType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);
        return NetworkEntitlementHelper.RemoveEntitlement(networkUser, key);
    }

    public static int? ReadEntitlement<T>(NetworkUser networkUser, EntitlementType entitlementWriterType)
    {
        var key = GetEntitlementKey(entitlementWriterType);
        var limits = new List<int?>
        {
            NetworkEntitlementHelper.ReadEntitlement<int?>(networkUser.Entitlements, key)
        };

        if (networkUser.NetworkUserRoles != null)
        {
            limits.AddRange(networkUser.NetworkUserRoles
                .Select(x => NetworkEntitlementHelper.ReadEntitlement<int?>(x.Role.Entitlements, key)));
        }

        return limits.Max();
    }
    
    private static string GetEntitlementKey(EntitlementType entitlementWriterType)
    {
        return entitlementWriterType switch
        {
            EntitlementType.BlogCountLimit => "BlogCountLimit",
            EntitlementType.ConfigurationCountLimit => "ConfigurationCountLimit",
            EntitlementType.CustomPageCountLimit => "CustomPageCountLimit",
            EntitlementType.CustomPageBlockCountLimit => "CustomPageBlockCountLimit",
            EntitlementType.CustomPageBlockSizeLimit => "CustomPageBlockSizeLimit",
            EntitlementType.FileSizeLimit => "FileSizeLimit",
            EntitlementType.FileCountLimit => "FileCountLimit",
            EntitlementType.FileStorageLimit => "FileStorageLimit",
            _ => throw new ArgumentOutOfRangeException(nameof(entitlementWriterType), entitlementWriterType, null)
        };
    }

}