using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

public class AuthenticatedDataLoader(IIdentityService identityService, IIdentityVerifier identityVerifier)
{
    // public class AppliedCache
    // {
    //     public Dictionary<string, HashSet<string>> history;
    //     public string previousCall;
    //     public UserProxy userProxy;
    // }
    //
    // public IQueryable<User> UserInclude(IQueryable<User> query, AppliedCache cache)
    // {
    //     const string callName = "UserInclude";
    //
    //     if (!cache.history.ContainsKey(callName))
    //         cache.history[callName] = [];
    //
    //     if (cache.history[callName].Contains(cache.previousCall)
    //         || !cache.history[cache.previousCall].Add(callName)
    //        )
    //         return query; // Prevent infinite loops
    //
    //     // Permission check for UserProxy
    //     if (identityVerifier.IsAllowed(cache.userProxy.Id, cache.userProxy.Id, Constants.Permissions.ReadUser))
    //     {
    //         query = query.Include(u => u.UserProxies);
    //
    //         foreach (var userProxyQuery in query.UserProxies)
    //         {
    //             query = UserProxyInclude(userProxyQuery, cache);
    //         }
    //
    //         query = UserProxyInclude(query, cache);
    //     }
    //
    //     return query;
    // }
    //
    // public IQueryable<UserProxy> UserProxyInclude(IQueryable<UserProxy> query, AppliedCache cache)
    // {
    //     if (!appliedIncludes.Add("NetworkInclude"))
    //         return query; // Prevent infinite loops
    //
    //     // Permission check for users
    //     if (currentUser.HasRole("CanSeeUsers"))
    //     {
    //         query = query.Include(n => n.NetworkUsers)
    //             .ThenInclude(nu => nu.User);
    //
    //         query = UserInclude(query.SelectMany(n => n.NetworkUsers.Select(nu => nu.User)), appliedIncludes);
    //     }
    //
    //     return query;
    // }
}