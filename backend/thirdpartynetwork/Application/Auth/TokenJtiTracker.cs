namespace thirdpartynetwork.Application.Auth;

public class JtiTracker
{
    public Guid Jti { get; init; }
    public JtiTracker? Next { get; set; }
    public JtiTracker? Previous { get; set; }
}

// TODO: Add timer to clean up tree.
// Have it check the timer, if expired, check tree for still active timers

public static class TokenJtiTracker
{
    private static readonly List<JtiTracker> JtiTree = [];
    
    public static Guid GenerateJti(Guid? previousJti)
    {
        var jti = Guid.NewGuid();

        var newTracker = new JtiTracker
        {
            Jti = jti
        };

        if (previousJti != null)
        {
            var previousTracker = JtiTree.Find(t => t.Jti == previousJti);
            if (previousTracker == null)
            {
                throw new UnauthorizedAccessException();
            }
            
            if (previousTracker.Previous!.Next != null)
            {
                RemoveSpanningTree(previousTracker);
                throw new UnauthorizedAccessException();
            }
            
            newTracker.Previous = previousTracker;
            previousTracker.Next = newTracker;
        }

        JtiTree.Add(newTracker);

        return jti;
    }

    public static bool ValidateJti(Guid jti)
    {
        return JtiTree.Any(t => t.Jti == jti);
    }

    private static void RemoveSpanningTree(JtiTracker tracker)
    {
        if (tracker.Previous != null) RemoveSpanningTree(tracker.Previous);
        if (tracker.Next != null) RemoveSpanningTree(tracker.Next);
        
        JtiTree.Remove(tracker);
    }
}
