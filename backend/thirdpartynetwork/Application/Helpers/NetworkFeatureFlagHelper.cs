using System.Text.Json;
using System.Text.Json.Nodes;
using thirdpartynetwork.Core.Models;

public static class NetworkFeatureFlagHelper
{
    public static NetworkUser WriteFeatureFlag<T>(NetworkUser networkUser, string key, T value)
    {
        var jsonString = networkUser.FeatureFlags.RootElement.GetRawText();
        var root = JsonNode.Parse(jsonString) ?? new JsonObject();

        root[key] = JsonValue.Create(value);
        networkUser.FeatureFlags = JsonDocument.Parse(root.ToJsonString());

        return networkUser;
    }

    public static T? ReadFeatureFlag<T>(JsonDocument? featureFlags, string key, T? defaultValue = default)
    {
        if (featureFlags == null) return defaultValue;
        if (featureFlags.RootElement.TryGetProperty(key, out var element))
        {
            return element.Deserialize<T>() ?? defaultValue;
        }

        return defaultValue;
    }

    public static NetworkUser RemoveFeatureFlag(NetworkUser networkUser, string key)
    {
        var root = JsonNode.Parse(networkUser.FeatureFlags.RootElement.GetRawText())?.AsObject();

        if (root != null && root.Remove(key))
        {
            // Only update the property if something was actually removed
            networkUser.FeatureFlags = JsonDocument.Parse(root.ToJsonString());
        }

        return networkUser;
    }

}