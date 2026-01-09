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
    private async Task<NetworkUser> GetUserByIdAsync(Guid networkId, Guid userProxyId)
    {
        var networkUser = await networkUserService.GetByIdsPreAuthAsync(networkId, userProxyId);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        return networkUser;
    }

    private async Task<NetworkUser> GetNetworkUserByEmailAsync(Guid networkId, string email)
    {
        var networkUser = await networkUserService.GetByNetworkIdAndEmailAsync(networkId, email);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        return networkUser;
    }

    private async Task<NetworkUser> CreateNetworkUserAsync(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        if (await networkUserService.GetByNetworkIdAndEmailAsync(networkId, createUserProxy.Email) != null)
            throw new ConflictHttpException("Email already used");
        var mainNetwork = await networkService.GetMainNetworkAsync();

        var userDto = new CreateUserDto();
        var user = await userService.CreateAsync(userDto);

        var userProxy = await userProxyService.CreateAsync(user.Id, createUserProxy);

        var networkUser = await networkUserService.CreateAsync(networkId, userProxy.Id, new CreateNetworkUserDto());
        if (mainNetwork.Id != networkId)
            await networkUserService.CreateAsync(mainNetwork.Id, userProxy.Id, new CreateNetworkUserDto());

        return networkUser;
    }

    public async Task<AuthorizationCodeDto> ExternalLoginAsync(Guid networkId, LoginDto loginData, string clientId, string codeChallenge)
    {
        var networkUser = await GetNetworkUserByEmailAsync(networkId, loginData.Email);
        if (!passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        var network = await networkService.GetByIdAsync(networkId, true);
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

    public async Task<TokensDto> RefreshAsync(Guid networkId, RefreshTokenDto refreshTokenDto, bool isExternal = true)
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
        var networkUser = await GetUserByIdAsync(networkId, userId);

        return isExternal 
            ? tokenGenerator.GenerateTokenPair(networkUser, audClaim?.Value, networkId, oldJti.Value)
            : tokenGenerator.GenerateTokenPair(networkUser, null, null, oldJti.Value);
    }

    public async Task<AuthorizationCodeDto> ExternalRegisterAsync(Guid networkId, CreateUserProxyDto createUserProxy, string clientId,
        string codeChallenge)
    {
        var networkUser = await CreateNetworkUserAsync(networkId, createUserProxy);

        var network = await networkService.GetByIdAsync(networkId);
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

    public async Task<TokensDto> TokenAsync(TokenRequestDto dto)
    {
        var authCode = AuthorizationCodeStore.Get(dto.Code);
        if (authCode == null)
            throw new UnauthorizedHttpException("Invalid or expired authorization code");

        if (authCode.ClientId != dto.ClientId)
            throw new UnauthorizedHttpException("Mismatched client ID");

        if (!PKCEVerifier.VerifyPKCE(dto.CodeVerifier, authCode.CodeChallenge))
            throw new UnauthorizedHttpException("Invalid code verifier");

        AuthorizationCodeStore.Remove(dto.Code);

        var networkUser = await GetUserByIdAsync(authCode.NetworkId, authCode.UserId);

        return tokenGenerator.GenerateTokenPair(networkUser, authCode.Audience, authCode.NetworkId);
    }

    public async Task<TokensDto> MainNetworkLoginAsync(LoginDto loginDto)
    {
        var mainNetwork = await networkService.GetMainNetworkAsync();

        return await LoginGetTokensAsync(mainNetwork.Id, loginDto);
    }

    private async Task<TokensDto> LoginGetTokensAsync(Guid networkId, LoginDto loginData)
    {
        var networkUser = await GetNetworkUserByEmailAsync(networkId, loginData.Email);
        if (networkUser == null) throw new UnauthorizedHttpException("Invalid email or password");

        if (networkUser.UserProxy.Password is null ||
            !passwordHasher.VerifyPassword(loginData.Password, networkUser.UserProxy.Password))
            throw new UnauthorizedHttpException("Invalid email or password");

        return tokenGenerator.GenerateTokenPair(networkUser);
    }

    public async Task<TokensDto> MainNetworkRefreshAsync(RefreshTokenDto refreshTokenDto)
    {
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return await RefreshAsync(mainNetwork.Id, refreshTokenDto, false);
    }

    public async Task<TokensDto> MainNetworkRegisterAsync(CreateUserProxyDto createUserProxy)
    {
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return await RegisterGetTokensAsync(mainNetwork.Id, createUserProxy);
    }

    private async Task<TokensDto> RegisterGetTokensAsync(Guid networkId, CreateUserProxyDto createUserProxy)
    {
        var networkUser = await CreateNetworkUserAsync(networkId, createUserProxy);
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