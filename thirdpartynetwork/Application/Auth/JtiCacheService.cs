using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class JtiCacheService : IJtiCacheService
{
    private readonly List<IJtiCacheService.JtiTracker> _jtiTrackers = [];

    public bool IsJtiValid(string jti)
    {
        return _jtiTrackers.Any(t => t.Jti == jti);
    }

    public void StoreJti(string jti, string? previousJti, TimeSpan expiry)
    {
        GarbageCollect();

        var newTracker = new IJtiCacheService.JtiTracker
        {
            Jti = jti,
            Expiration = DateTime.UtcNow.Add(expiry)
        };

        if (previousJti != null) AppendValidateJti(newTracker, previousJti);

        _jtiTrackers.Add(newTracker);
    }

    private void AppendValidateJti(IJtiCacheService.JtiTracker newTracker, string previousJti)
    {
        var previousTracker = _jtiTrackers.Find(t => t.Jti == previousJti);
        if (previousTracker == null)
        {
            throw new UnauthorizedAccessException();
        }

        if (previousTracker.Next != null)
        {
            RemoveTrackerTree(previousTracker);
            throw new UnauthorizedAccessException();
        }

        previousTracker.Next = newTracker;
        previousTracker.Expiration = null;
    }

    public void RemoveJti(string jti)
    {
        GarbageCollect();

        var tracker = _jtiTrackers.Find(t => t.Jti == jti);
        if (tracker == null) return;
        RemoveTrackerTree(tracker);
    }

    private void RemoveTrackerTree(IJtiCacheService.JtiTracker tracker)
    {
        if (tracker.Next != null) RemoveTrackerTree(tracker.Next);
        
        var childTrackers = _jtiTrackers.FindAll(t => t.Next == tracker);
        foreach (var childTracker in childTrackers)
        {
            RemoveTrackerTree(childTracker);
        }

        _jtiTrackers.Remove(tracker);
    }

    private void GarbageCollect()
    {
        var expiredTrackers = _jtiTrackers
            .Where(t => t.Expiration != null && t.Expiration < DateTime.UtcNow)
            .ToList();
        
        foreach (var expiredTracker in expiredTrackers)
        {
            RemoveTrackerTree(expiredTracker);
        }
    }
}
