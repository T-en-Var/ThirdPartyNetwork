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
    IUserService userService) : IAuthService
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

    private NetworkUser CreateNetworkUser(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        if (networkUserService.GetByNetworkIdAndEmail(networkId, createUserProxy.Email) != null)
            throw new ConflictHttpException("Email already used");
        var mainNetwork = networkService.GetMainNetwork();

        var userDto = new CreateUserDto();
        var user = userService.Create(userDto);

        var userProxy = userProxyService.Create(user.Id, createUserProxy);

        var networkUser = networkUserService.Create(networkId, userProxy.Id, new CreateNetworkUserDto());
        if (mainNetwork.Id != networkId)
            networkUserService.Create(mainNetwork.Id, userProxy.Id, new CreateNetworkUserDto());

        return networkUser;
    }

    public AuthorizationCodeDto ExternalLogin(Guid networkId, LoginDto loginData, string clientId, string codeChallenge)
    {
        var networkUser = GetNetworkUserByEmail(networkId, loginData.Email);
        if (!passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        var network = networkService.GetById(networkId, true);
        if (network == null) throw new NotFoundHttpException("Invalid email or password");

        var audience = GetAudienceFromRedirectUri(network.RedirectURI);
        if (audience == null)
            throw new BadHttpRequestException(
                "Could not determine audience from redirect URI. Please contact your administrator.");

        var code = Guid.NewGuid().ToString("N");
        var tokens = tokenGenerator.GenerateTokenPair(networkUser, audience, networkId);

        AuthorizationCodeStore.Store(new AuthorizationCode
        {
            Code = code,
            UserId = networkUser.UserProxy.Id,
            NetworkId = networkId,
            ClientId = clientId,
            CodeChallenge = codeChallenge,
            Audience = audience,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        return new AuthorizationCodeDto { Code = code, AccessToken = tokens.AccessToken };
    }

    public TokensDto Refresh(Guid networkId, RefreshTokenDto refreshTokenDto, bool isExternal = true)
    {
        var isTokenValid = jwtHandler.ValidateToken(refreshTokenDto.RefreshToken);
        if (!isTokenValid) throw new UnauthorizedHttpException("Invalid token 1");

        var oldToken = jwtHandler.DecodeToken(refreshTokenDto.RefreshToken);
        var uidClaim = oldToken.Claims.FirstOrDefault(x => x.Type == "uid");
        var audClaim = oldToken.Claims.FirstOrDefault(x => x.Type == "aud");
        var oldJti = oldToken.Claims.FirstOrDefault(x => x.Type == "jti");

        if (uidClaim == null) throw new UnauthorizedHttpException("Invalid token 2");
        if (isExternal && audClaim == null) throw new UnauthorizedHttpException("Invalid token 2.5");
        if (oldJti == null) throw new UnauthorizedHttpException("Invalid token 3");

        var userId = Guid.Parse(uidClaim.Value);
        var networkUser = GetUserById(networkId, userId);

        return isExternal 
            ? tokenGenerator.GenerateTokenPair(networkUser, audClaim?.Value, networkId, oldJti.Value)
            : tokenGenerator.GenerateTokenPair(networkUser, null, null, oldJti.Value);
    }

    public AuthorizationCodeDto ExternalRegister(Guid networkId, CreateUserProxyDto createUserProxy, string clientId,
        string codeChallenge)
    {
        var networkUser = CreateNetworkUser(networkId, createUserProxy);

        var network = networkService.GetById(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var audience = GetAudienceFromRedirectUri(network.RedirectURI);
        if (audience == null)
            throw new BadHttpRequestException(
                "Could not determine audience from redirect URI. Please contact your administrator.");

        var code = Guid.NewGuid().ToString("N");
        var tokens = tokenGenerator.GenerateTokenPair(networkUser);

        AuthorizationCodeStore.Store(new AuthorizationCode
        {
            Code = code,
            UserId = networkUser.UserProxyId,
            NetworkId = networkId,
            ClientId = clientId,
            CodeChallenge = codeChallenge,
            Audience = audience,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        return new AuthorizationCodeDto { Code = code, AccessToken = tokens.AccessToken };
    }

    public TokensDto Token(TokenRequestDto dto)
    {
        var authCode = AuthorizationCodeStore.Get(dto.Code);
        if (authCode == null)
            throw new UnauthorizedHttpException("Invalid or expired authorization code");

        if (authCode.ClientId != dto.ClientId)
            throw new UnauthorizedHttpException("Mismatched client ID");

        if (!PKCEVerifier.VerifyPKCE(dto.CodeVerifier, authCode.CodeChallenge))
            throw new UnauthorizedHttpException("Invalid code verifier");

        AuthorizationCodeStore.Remove(dto.Code);

        var networkUser = GetUserById(authCode.NetworkId, authCode.UserId);

        return tokenGenerator.GenerateTokenPair(networkUser, authCode.Audience, authCode.NetworkId);
    }

    public TokensDto MainNetworkLogin(LoginDto loginDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return LoginGetTokens(mainNetwork.Id, loginDto);
    }

    private TokensDto LoginGetTokens(Guid networkId, LoginDto loginData)
    {
        var networkUser = GetNetworkUserByEmail(networkId, loginData.Email);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        if (networkUser.UserProxy.Password is null ||
            !passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        return tokenGenerator.GenerateTokenPair(networkUser);
    }

    public TokensDto MainNetworkRefresh(RefreshTokenDto refreshTokenDto)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return Refresh(mainNetwork.Id, refreshTokenDto, false);
    }

    public TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy)
    {
        var mainNetwork = networkService.GetMainNetwork();
        return RegisterGetTokens(mainNetwork.Id, createUserProxy);
    }

    private TokensDto RegisterGetTokens(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        var networkUser = CreateNetworkUser(networkId, createUserProxy);
        return tokenGenerator.GenerateTokenPair(networkUser);
    }

    private static string? GetAudienceFromRedirectUri(string redirectUri)
    {
        if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
        {
            // From: "http://localhost:3005/callback", get: "http://localhost:3005"
            return $"{uri.Scheme}://{uri.Authority}";
        }
        
        return null;
    }
}