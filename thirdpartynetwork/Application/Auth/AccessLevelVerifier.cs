using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
    public async Task<bool> CheckFileAccessAsync(string fileId, Guid networkId, string permissionName)
    {
        var file = await dataContext.Files.FirstOrDefaultAsync(x => x.Id == ValueToGuid(fileId));
        return await CheckAccessAsync(file, networkId, permissionName);
    }

    public async Task<bool> CheckCustomPageAccessAsync(string customPageId, Guid networkId, string permissionName)
    {
        var customPage = await dataContext.CustomPages.FirstOrDefaultAsync(x => x.Id == ValueToGuid(customPageId));
        return await CheckAccessAsync(customPage, networkId, permissionName);
    }

    public async Task<bool> CheckPageBlockAccessAsync(string pageBlockId, Guid networkId, string permissionName)
    {
        var pageBlock = await dataContext.PageBlocks.FirstOrDefaultAsync(x => x.Id == ValueToGuid(pageBlockId));
        // Check parent for permissions
        return pageBlock != null && await CheckCustomPageAccessAsync(pageBlock.CustomPageId.ToString(), networkId, permissionName);
    }

    public async Task<bool> CheckConfigurationAccessAsync(string configurationId, Guid networkId, string permissionName)
    {
        var configuration = await dataContext.Configurations.FirstOrDefaultAsync(x => x.Id == ValueToGuid(configurationId));
        return await CheckAccessAsync(configuration, networkId, permissionName);
    }

    public async Task<bool> CheckBlogAccessAsync(string blogCompareValue, Guid networkId, string permissionName)
    {
        var blogId = ValueToGuid(blogCompareValue);
        var blog = blogId != Guid.Empty 
            ? await dataContext.Blogs.FirstOrDefaultAsync(x => x.Id == blogId)
            : await dataContext.Blogs.FirstOrDefaultAsync(x => x.Slug == blogCompareValue);

        return await CheckAccessAsync(blog, networkId, permissionName);
    }

    public async Task<bool> CheckAccessAsync(UserContentBaseModel? model, Guid networkId, string permissionName)
    {
        if (model == null) return false;
        
        if (model.AccessLevel == AccessLevel.Public) return true;

        // If resource is not public, you must be authenticated
        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);
        if (userProxy?.User == null) return false;

        // If user has permission in either super network or local network
        if (await identityVerifier.IsAllowedAsync(networkId, userProxy.Id, permissionName))
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

    public async Task<Expression<Func<T, bool>>> GetAccessPredicateAsync<T>(Guid requestedNetworkId, string permissionName)
        where T : UserContentBaseModel
    {
        // --- STEP 1: PRE-CALCULATE CONTEXT (C# Side) ---
        // We calculate "Who is the user?" and "What can they do?" here 
        // because SQL cannot run C# methods like 'IsAllowed'.

        Guid? currentUserId = null;
        var isMemberOfRequestedNetwork = false;
        var hasPermission = false;

        // Get the user (safe mode, no exceptions)
        var userProxy = await identityService.GetUserProxyAsync(omitThrow: true);

        // Check if it's a real, authenticated user
        var isRealUser = userProxy?.User != null;

        if (isRealUser)
        {
            currentUserId = userProxy!.Id;

            // 1. Check Network Membership
            if (userProxy.NetworkUsers != null)
            {
                isMemberOfRequestedNetwork = userProxy.NetworkUsers
                    .Any(nu => nu.NetworkId == requestedNetworkId);
            }


            // 2. Check Permissions (The "Super Admin" / Override Check)
            // We calculate this once in C#. If true, it effectively bypasses 
            // the checks for Private/Network in the SQL later.
            hasPermission = await identityVerifier.IsAllowedAsync(requestedNetworkId, currentUserId.Value, permissionName);
        }
        
        Console.WriteLine($"hasPermission: {hasPermission}");
        Console.WriteLine($"isRealUser: {isRealUser}");
        Console.WriteLine($"userProxy.Id: {userProxy?.Id}");
        Console.WriteLine($"isMemberOfRequestedNetwork: {isMemberOfRequestedNetwork}");
        
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

    private static Guid ValueToGuid(string value)
    {
        return Guid.TryParse(value, out var result) ? result : Guid.Empty;
    }
}