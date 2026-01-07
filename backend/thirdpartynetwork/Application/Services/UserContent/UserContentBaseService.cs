using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;


public abstract class UserContentBaseService<T>(IUserContentBaseRepository<T> repository, INetworkUserService networkUserService) 
    : IUserContentBaseService<T> 
    where T : UserContentBaseModel
{
    public ICollection<T> GetByAuthorId(Guid networkUserId)
    {
        var networkUser = networkUserService.GetJustById(networkUserId);
        if (networkUser == null) throw new NotFoundHttpException();
        return repository.GetByAuthorId(networkUser.NetworkId, networkUserId);
    }
    
    public ICollection<T> GetByUserProxyId(Guid userProxyId)
    {
        return repository.GetByUserProxyId(userProxyId);
    }
}