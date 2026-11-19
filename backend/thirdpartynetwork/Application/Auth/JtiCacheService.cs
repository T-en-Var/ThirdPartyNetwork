using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class JtiCacheService : IJtiCacheService
{
    private readonly object _lock = new();

    private readonly Dictionary<string, IJtiCacheService.JtiTracker> _trackers = new();

    public bool IsJtiValid(string jti)
    {
        lock (_lock)
        {
            return _trackers.ContainsKey(jti);
        }
    }

    public void StoreJti(string jti, string? previousJti, TimeSpan expiry)
    {
        Console.WriteLine($"StoreJti: {jti} from {previousJti}");
        lock (_lock)
        {
            GarbageCollect();

            var newTracker = new IJtiCacheService.JtiTracker
            {
                Jti = jti,
                Expiration = DateTime.UtcNow.Add(expiry)
            };

            if (previousJti != null)
            {
                AppendValidateJti(newTracker, previousJti);
            }

            _trackers[jti] = newTracker;
        }
    }

    private void AppendValidateJti(IJtiCacheService.JtiTracker newTracker, string previousJti)
    {
        if (!_trackers.TryGetValue(previousJti, out var previousTracker))
        {
            throw new UnauthorizedHttpException("Invalid token 4 (jti not found)");
        }

        if (previousTracker.Next != null)
        {
            RemoveTrackerTree(previousTracker.Jti);
            throw new UnauthorizedHttpException("Invalid token 5 (jti reuse detected)");
        }

        previousTracker.Next = newTracker;
    }

    public void RemoveJti(string jti)
    {
        lock (_lock)
        {
            GarbageCollect();
            RemoveTrackerTree(jti);
        }
    }

    private void RemoveTrackerTree(string jti)
    {
        if (!_trackers.Remove(jti, out var tracker))
            return;

        if (tracker.Next != null)
        {
            RemoveTrackerTree(tracker.Next.Jti);
        }

        var parents = _trackers.Values.Where(t => t.Next == tracker).ToList();
        foreach (var parent in parents)
        {
            RemoveTrackerTree(parent.Jti);
        }
    }

    private void GarbageCollect()
    {
        var now = DateTime.UtcNow;

        var expiredJtis = _trackers.Values
            .Where(t => t.Expiration < now)
            .Select(t => t.Jti)
            .ToList();

        foreach (var jti in expiredJtis)
        {
            RemoveTrackerTree(jti);
        }
    }
}