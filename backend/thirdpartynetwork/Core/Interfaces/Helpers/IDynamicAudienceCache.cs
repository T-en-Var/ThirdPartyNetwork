namespace thirdpartynetwork.Core.Interfaces.Helpers;

public interface IDynamicAudienceCache
{
    public void ReloadAudiences(IEnumerable<string> redirectUris);
    public bool IsValidAudience(string? audience);
}