using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;


public abstract class UserContentBaseService<T>(IUserContentBaseRepository<T> repository, INetworkUserService networkUserService) 
    : IUserContentBaseService<T> 
    where T : UserContentBaseModel
{
    public async Task<ICollection<T>> GetByAuthorIdAsync(Guid networkUserId)
    {
        var networkUser = await networkUserService.GetJustByIdAsync(networkUserId);
        if (networkUser == null) throw new NotFoundHttpException();
        return await repository.GetByAuthorIdAsync(networkUser.NetworkId, networkUserId);
    }
    
    public async Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId)
    {
        return await repository.GetByUserProxyIdAsync(userProxyId);
    }
}