using System.Linq.Expressions;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class AccessLevelVerifier(
    DataContext dataContext,
    IIdentityService identityService,
    IIdentityVerifier identityVerifier
) : IAccessLevelVerifier
{
    public bool CheckFileAccess(Guid fileId, Guid networkId, string permissionName)
    {
        var file = dataContext.Files.FirstOrDefault(x => x.Id == fileId);
        return file != null && CheckAccess(file, networkId, permissionName);
    }

    public bool CheckCustomPageAccess(Guid customPageId, Guid networkId, string permissionName)
    {
        var customPage = dataContext.CustomPages.FirstOrDefault(x => x.Id == customPageId);
        return customPage != null && CheckAccess(customPage, networkId, permissionName);
    }

    public bool CheckPageBlockAccess(Guid pageBlockId, Guid networkId, string permissionName)
    {
        var pageBlock = dataContext.PageBlocks.FirstOrDefault(x => x.Id == pageBlockId);
        // Check parent for permissions
        return pageBlock != null && CheckCustomPageAccess(pageBlock.CustomPageId, networkId, permissionName);
    }

    public bool CheckConfigurationAccess(Guid configurationId, Guid networkId, string permissionName)
    {
        var configuration = dataContext.Configurations.FirstOrDefault(x => x.Id == configurationId);
        return configuration != null && CheckAccess(configuration, networkId, permissionName);
    }

    public bool CheckAccess(UserContentBaseModel model, Guid networkId, string permissionName)
    {
        if (model.AccessLevel == AccessLevel.Public) return true;

        // If resource is not public, you must be authenticated
        var userProxy = identityService.GetUserProxy(omitThrow: true);
        if (userProxy?.User == null) return false;

        // If user has permission in either super network or local network
        if (identityVerifier.IsAllowed(networkId, userProxy.Id, permissionName))
        {
            return true;
        }

        if (model.AccessLevel == AccessLevel.Private)
        {
            return model.AuthorId == userProxy.Id;
        }

        // If user in same network as resource
        if (model.AccessLevel == AccessLevel.PublicToNetwork)
        {
            return model.NetworkId == networkId && userProxy.NetworkUsers.Any(u => u.NetworkId == networkId);
        }

        return false;
    }

    public Expression<Func<T, bool>> GetAccessPredicate<T>(Guid requestedNetworkId, string permissionName)
        where T : UserContentBaseModel
    {
        // --- STEP 1: PRE-CALCULATE CONTEXT (C# Side) ---
        // We calculate "Who is the user?" and "What can they do?" here 
        // because SQL cannot run C# methods like 'IsAllowed'.

        Guid? currentUserId = null;
        var isMemberOfRequestedNetwork = false;
        var hasPermission = false;

        // Get the user (safe mode, no exceptions)
        var userProxy = identityService.GetUserProxy(omitThrow: true);

        // Check if it's a real, authenticated user
        var isRealUser = userProxy?.User != null;
        Console.WriteLine($"User: {userProxy?.Username ?? "no name"}");

        if (isRealUser)
        {
            currentUserId = userProxy!.Id;

            // 1. Check Network Membership
            if (userProxy.NetworkUsers != null)
            {
                Console.WriteLine($"userProxy.NetworkUsers: {userProxy!.NetworkUsers.Count}");
                isMemberOfRequestedNetwork = userProxy.NetworkUsers
                    .Any(nu => nu.NetworkId == requestedNetworkId);
            }
            

            // 2. Check Permissions (The "Super Admin" / Override Check)
            // We calculate this once in C#. If true, it effectively bypasses 
            // the checks for Private/Network in the SQL later.
            hasPermission = identityVerifier.IsAllowed(requestedNetworkId, currentUserId.Value, permissionName);
        }
        Console.WriteLine($"isMemberOfRequestedNetwork: {isMemberOfRequestedNetwork}");
        Console.WriteLine($"hasPermission: {hasPermission}");

        // --- STEP 2: BUILD EXPRESSION (SQL Side) ---
        // EF Core converts this Lambda into SQL.
        // Boolean variables (like hasPermission) are passed as parameters (0 or 1).

        return model =>
            // 1. Public: Everyone can see it
            model.AccessLevel == AccessLevel.Public

            // 2. Permission Override:
            // Matches your CheckAccess logic: "if (IsAllowed) return true;"
            || hasPermission

            // 3. PublicToNetwork:
            // Must match network AND user must be a member
            || (model.AccessLevel == AccessLevel.PublicToNetwork
                && model.NetworkId == requestedNetworkId
                && isMemberOfRequestedNetwork)

            // 4. Private:
            // Must be the author
            || (model.AccessLevel == AccessLevel.Private
                && currentUserId.HasValue
                && model.AuthorId == currentUserId.Value);
    }
}