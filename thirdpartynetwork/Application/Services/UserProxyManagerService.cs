using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class UserProxyManagerService(
    IUserService userService,
    INetworkService networkService,
    INetworkUserService networkUserService,
    IUserProxyService userProxyService) : IUserProxyManagerService
{
    public async Task<UserProxy> AddUserProxyAsync(Guid userId, AddUserProxyDto addUserProxy)
    {
        var user = await userService.GetByIdAsync(userId);
        if (user == null) throw new NotFoundHttpException("User not found");

        var mainNetwork = await networkService.GetMainNetworkAsync();

        if (addUserProxy.Email != null && await networkUserService.GetByNetworkIdAndEmailAsync(mainNetwork.Id, addUserProxy.Email) != null)
            throw new ConflictHttpException("Email already used");

        var createUserProxyDto = new CreateUserProxyDto()
        {
            Email = addUserProxy.Email,
            Password = addUserProxy.Password,
            Username = addUserProxy.Username,
            FirstName = addUserProxy.FirstName,
            LastName = addUserProxy.LastName,
        };
        
        var userProxy = await userProxyService.CreateAsync(user.Id, createUserProxyDto);

        if (addUserProxy.FileLink != null)
        {
            userProxy.ImageFileId = addUserProxy.FileLink.Id;
            // this *should* be saved in networkUserService.Create
        }

        await networkUserService.CreateAsync(mainNetwork.Id, userProxy.Id, new CreateNetworkUserDto());

        return userProxy;
    }
}