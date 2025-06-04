using System.Security.Claims;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Stores;

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
    private NetworkUser GetUserById(Guid networkId, Guid userProxyId)
    {
        var networkUser = networkUserService.GetByIdsPreAuth(networkId, userProxyId);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");
        
        return networkUser;
    }

    private NetworkUser GetNetworkUserByEmail(Guid networkId, string email)
    {
        var networkUser = networkUserService.GetByNetworkIdAndEmail(networkId, email);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        return networkUser;
    }

    public AuthorizationCodeDto Login(Guid networkId, LoginDto loginData, string clientId, string redirectUri, string codeChallenge)
    {
        var networkUser = GetNetworkUserByEmail(networkId, loginData.Email);
        if (!passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        var code = Guid.NewGuid().ToString("N");
        var tokens = tokenGenerator.GenerateTokenPair(networkUser);

        AuthorizationCodeStore.Store(new AuthorizationCode
        {
            Code = code,
            UserId = networkUser.UserProxy.Id,
            NetworkId = networkId,
            ClientId = clientId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        return new AuthorizationCodeDto { Code = code, AccessToken = tokens.AccessToken };
    }
    
    public TokensDto Refresh(Guid networkId, RefreshTokenDto refreshTokenDto)
    {
        if (httpContextAccessor.HttpContext == null) throw new UnauthorizedHttpException("No HttpContext User");

        var appUser = httpContextAccessor.HttpContext.User;
        if (appUser == null) throw new UnauthorizedHttpException("No HttpContext User");

        var uidClaim = appUser.Claims.FirstOrDefault(x => x.Type == "uid");
        if (uidClaim == null) throw new UnauthorizedHttpException("Invalid token 1");

        var userId = Guid.Parse(uidClaim.Value);
        var networkUser = GetUserById(networkId, userId);
        
        var isTokenInvalid = jwtHandler.ValidateToken(refreshTokenDto.RefreshToken);
        var isTokenPairValid = jwtHandler.ValidateTokenPair(appUser, refreshTokenDto.RefreshToken);

        if (!isTokenInvalid || !isTokenPairValid) throw new UnauthorizedHttpException("Invalid token 2");

        var oldToken = jwtHandler.DecodeToken(refreshTokenDto.RefreshToken);
        var oldJti = oldToken.Claims.FirstOrDefault(x => x.Type == "jti");

        if (oldJti == null) throw new UnauthorizedHttpException("Invalid token 3");
        
        Console.WriteLine("OldJti: " + oldJti.Value);

        return tokenGenerator.GenerateTokenPair(networkUser, oldJti.Value);
    }

    public AuthorizationCodeDto Register(Guid networkId, CreateUserProxyDto createUserProxy, string clientId, string redirectUri, string codeChallenge)
    {
        var userProxy = CreateUserProxy(networkId, createUserProxy);
        var code = Guid.NewGuid().ToString("N");

        AuthorizationCodeStore.Store(new AuthorizationCode
        {
            Code = code,
            UserId = userProxy.Id,
            NetworkId = networkId,
            ClientId = clientId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        return new AuthorizationCodeDto { Code = code };
    }
    
    public TokensDto Token(TokenRequestDto dto)
    {
        var authCode = AuthorizationCodeStore.Get(dto.Code);
        if (authCode == null)
            throw new UnauthorizedHttpException("Invalid or expired authorization code");

        if (authCode.RedirectUri != dto.RedirectUri)
            throw new UnauthorizedHttpException("Mismatched redirect URI");

        if (authCode.ClientId != dto.ClientId)
            throw new UnauthorizedHttpException("Mismatched client ID");

        if (!PKCEVerifier.VerifyPKCE(dto.CodeVerifier, authCode.CodeChallenge))
            throw new UnauthorizedHttpException("Invalid code verifier");

        // Consume the code (one-time use)
        AuthorizationCodeStore.Remove(dto.Code);

        var networkUser = GetUserById(authCode.NetworkId, authCode.UserId);
        
        return tokenGenerator.GenerateTokenPair(networkUser);
    }

    public TokensDto MainNetworkLogin(LoginDto loginDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return LoginGetTokens(mainNetwork.Id, loginDto);
    }

    public TokensDto MainNetworkRefresh(RefreshTokenDto refreshTokenDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return Refresh(mainNetwork.Id, refreshTokenDto);
    }

    public TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return RegisterGetTokens(mainNetwork.Id, createUserProxy);
    }

    private UserProxy CreateUserProxy(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        if (networkUserService.GetByNetworkIdAndEmail(networkId, createUserProxy.Email) != null) throw new ConflictHttpException("Email already used");
        
        var userDto = new CreateUserDto() { };
        var user = userService.Create(userDto);

        return userProxyService.Create(user.Id, createUserProxy);
    }

    private TokensDto RegisterGetTokens(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        var userProxy = CreateUserProxy(networkId, createUserProxy);

        var networkUserDto = new CreateNetworkUserDto() { };
        var networkUser = networkUserService.Create(networkId, userProxy.Id, networkUserDto);

        return tokenGenerator.GenerateTokenPair(networkUser);
    }

    private TokensDto LoginGetTokens(Guid networkId, LoginDto loginData)
    {
        var networkUser = GetNetworkUserByEmail(networkId, loginData.Email);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        if (!passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        return tokenGenerator.GenerateTokenPair(networkUser);
    }
}