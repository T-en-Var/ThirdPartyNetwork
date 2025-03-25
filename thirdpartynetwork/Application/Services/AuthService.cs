using System.Security.Claims;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class AuthService(
    ITokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher,
    IJwtHandler jwtHandler,
    INetworkService networkService,
    INetworkUserService networkUserService,
    IUserProxyService userProxyService,
    IUserService userService,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private UserProxy GetUserById(Guid networkId, Guid userProxyId)
    {
        var networkUser = networkUserService.GetByIds(networkId, userProxyId);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid username or password");

        return networkUser.UserProxy;
    }

    private UserProxy GetUserByUsername(Guid networkId, string username)
    {
        var networkUser = networkUserService.GetByNetworkIdAndUsername(networkId, username);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid username or password");

        return networkUser.UserProxy;
    }

    public TokensDto Login(Guid networkId, LoginDto loginData)
    {
        var user = GetUserByUsername(networkId, loginData.Username);

        if (!passwordHasher.VerifyPassword(loginData.Password, user.Password))
            throw new UnauthorizedHttpException("Invalid username or password");

        return tokenGenerator.GenerateTokenPair(user.Id, user.Username);
    }

    public TokensDto Refresh(Guid networkId, RefreshTokenDto refreshTokenDto)
    {
        if (httpContextAccessor.HttpContext == null) throw new UnauthorizedHttpException("No HttpContext User");

        var appUser = httpContextAccessor.HttpContext.User;
        if (appUser == null) throw new UnauthorizedHttpException("No HttpContext User");

        var uidClaim = appUser.Claims.FirstOrDefault(x => x.Type == "uid");
        if (uidClaim == null) throw new UnauthorizedHttpException("Invalid token 1");

        var userId = Guid.Parse(uidClaim.Value);
        var user = GetUserById(networkId, userId);
        
        var isTokenInvalid = jwtHandler.ValidateToken(refreshTokenDto.RefreshToken);
        var isTokenPairValid = jwtHandler.ValidateTokenPair(appUser, refreshTokenDto.RefreshToken);

        if (!isTokenInvalid || !isTokenPairValid) throw new UnauthorizedHttpException("Invalid token 2");

        var oldToken = jwtHandler.DecodeToken(refreshTokenDto.RefreshToken);
        var oldJti = oldToken.Claims.FirstOrDefault(x => x.Type == "jti");

        if (oldJti == null) throw new UnauthorizedHttpException("Invalid token 3");

        return tokenGenerator.GenerateTokenPair(user.Id, user.Username, oldJti.Value);
    }

    public TokensDto Register(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        var userDto = new CreateUserDto() { };
        var user = userService.Create(userDto);

        var userProxy = userProxyService.Create(user.Id, createUserProxy);
        var networkUserDto = new CreateNetworkUserDto() { };

        networkUserService.Create(networkId, userProxy.Id, networkUserDto);

        return tokenGenerator.GenerateTokenPair(user.Id, userProxy.Username);
    }

    public TokensDto MainNetworkLogin(LoginDto loginDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return Login(mainNetwork.Id, loginDto);
    }

    public TokensDto MainNetworkRefresh(RefreshTokenDto refreshTokenDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return Refresh(mainNetwork.Id, refreshTokenDto);
    }

    public TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return Register(mainNetwork.Id, createUserProxy);
    }
}