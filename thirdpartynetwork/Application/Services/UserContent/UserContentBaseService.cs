using thirdpartynetwork.Core.Enum;
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
        return await repository.GetByAuthorIdAsync(networkUser.NetworkId, networkUser.UserProxyId);
    }

    public async Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId)
    {
        return await repository.GetByUserProxyIdAsync(userProxyId);
    }

    public async Task<T> PublishAsync(T entity)
    {
        entity.PublishedAt = DateTime.UtcNow;
        await repository.SaveAndUpdateAsync(entity);
        return entity;
    }

    public async Task<T> UnpublishAsync(T entity)
    {
        entity.PublishedAt = null;
        await repository.SaveAndUpdateAsync(entity);
        return entity;
    }

    public async Task<T> SetOwnershipModeAsync(T entity, OwnershipMode mode)
    {
        entity.OwnershipMode = mode;
        await repository.SaveAndUpdateAsync(entity);
        return entity;
    }
}
