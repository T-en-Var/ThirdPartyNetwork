using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class IdentityVerifier(IContentService contentService, IUserProxyService userProxyService) : IIdentityVerifier
{
    public bool IsContentOwner(Guid userProxyId, Guid targetContentId)
    {
        var content = contentService.GetById(targetContentId);
        return content != null && content.CreatedBy == userProxyId;
    }

    public bool IsUserOwner(Guid userProxyId, Guid targetUserId)
    {
        var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        
        return userProxyQueryable.Any(u => u.User.Id == targetUserId);
    }

    public bool IsUserProxyOwner(Guid userProxyId, Guid targetUserProxyId)
    {
        if (userProxyId == targetUserProxyId) return true;
        
        var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        
        return userProxyQueryable.Any(u => 
            u.User.Proxies.Any(n => n.Id == targetUserProxyId));
    }

    public bool IsNetworkUserOwner(Guid userProxyId, Guid targetNetworkUserId)
    {
        var userProxyQueryable = userProxyService.GetByIdQueryable(userProxyId);
        
        return userProxyQueryable.Any(n => 
            n.NetworkUsers.Any(u => u.Id == targetNetworkUserId));
    }
}
