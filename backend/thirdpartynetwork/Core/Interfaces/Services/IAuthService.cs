namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IAuthService : IBaseService
{
    Task<AuthorizationCodeDto> ExternalLoginAsync(Guid networkId, LoginDto loginData, string clientId, string codeChallenge);
    Task<TokensDto> RefreshAsync(Guid networkId, RefreshTokenDto refreshToken, bool isExternal = true);
    Task<AuthorizationCodeDto> ExternalRegisterAsync(Guid networkId, CreateUserProxyDto createUserProxy, string clientId, string codeChallenge);
    Task<TokensDto> TokenAsync(TokenRequestDto dto);
    Task<TokensDto> MainNetworkLoginAsync(LoginDto login);
    Task<TokensDto> MainNetworkRefreshAsync(RefreshTokenDto refreshToken);
    Task<TokensDto> MainNetworkRegisterAsync(CreateUserProxyDto createUserProxy);
}
