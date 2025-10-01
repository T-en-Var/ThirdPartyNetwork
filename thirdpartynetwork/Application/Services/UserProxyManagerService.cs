using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class UserProxyManagerService(
    IUserService userService,
    INetworkService networkService,
    INetworkUserService networkUserService,
    IUserProxyService userProxyService,
    IPasswordHasher passwordHasher,
    IMapper mapper) : IUserProxyManagerService
{
    public UserProxy AddUserProxy(Guid userId, AddUserProxyDto addUserProxy)
    {
        var user = userService.GetById(userId);
        if (user == null) throw new NotFoundHttpException("User not found");

        var mainNetwork = networkService.GetMainNetwork();

        if (addUserProxy.Email != null &&
            networkUserService.GetByNetworkIdAndEmail(mainNetwork.Id, addUserProxy.Email) != null)
            throw new ConflictHttpException("Email already used");

        var createUserProxyDto = new CreateUserProxyDto()
        {
            Email = addUserProxy.Email,
            Password = addUserProxy.Password,
            Username = addUserProxy.Username,
            FirstName = addUserProxy.FirstName,
            LastName = addUserProxy.LastName,
        };
        
        var userProxy = userProxyService.Create(user.Id, createUserProxyDto);

        if (addUserProxy.FileLink != null)
        {
            userProxy.ImageFileId = addUserProxy.FileLink.Id;
            // this *should* be saved in networkUserService.Create
        }

        networkUserService.Create(mainNetwork.Id, userProxy.Id, new CreateNetworkUserDto());

        return userProxy;
    }
}