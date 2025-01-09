using Microsoft.Extensions.Caching.Distributed;

namespace thirdpartynetwork.Application.Auth;

public class JtiCacheService
{
    private readonly IDistributedCache _cache;

    public JtiCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsJtiValidAsync(string jti)
    {
        return await _cache.GetStringAsync(jti) != null;
    }

    public async Task StoreJtiAsync(string jti, TimeSpan expiry)
    {
        await _cache.SetStringAsync(jti, "used", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        });
    }

    public async Task RemoveJtiAsync(string jti)
    {
        await _cache.RemoveAsync(jti);
    }
}
