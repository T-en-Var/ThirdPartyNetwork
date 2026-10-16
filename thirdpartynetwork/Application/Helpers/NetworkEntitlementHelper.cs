using System.Text.Json;
using System.Text.Json.Nodes;
using thirdpartynetwork.Core.Models;

public static class NetworkEntitlementHelper
{
    public static NetworkUser WriteEntitlement<T>(NetworkUser networkUser, string key, T value)
    {
        var jsonString = networkUser.Entitlements.RootElement.GetRawText();
        var root = JsonNode.Parse(jsonString) ?? new JsonObject();

        root[key] = JsonValue.Create(value);
        networkUser.Entitlements = JsonDocument.Parse(root.ToJsonString());

        return networkUser;
    }

    public static T? ReadEntitlement<T>(JsonDocument? entitlements, string key, T? defaultValue = default)
    {
        if (entitlements == null) return defaultValue;
        if (entitlements.RootElement.TryGetProperty(key, out var element))
        {
            return element.Deserialize<T>() ?? defaultValue;
        }

        return defaultValue;
    }

    public static NetworkUser RemoveEntitlement(NetworkUser networkUser, string key)
    {
        var root = JsonNode.Parse(networkUser.Entitlements.RootElement.GetRawText())?.AsObject();

        if (root != null && root.Remove(key))
        {
            // Only update the property if something was actually removed
            networkUser.Entitlements = JsonDocument.Parse(root.ToJsonString());
        }

        return networkUser;
    }

}