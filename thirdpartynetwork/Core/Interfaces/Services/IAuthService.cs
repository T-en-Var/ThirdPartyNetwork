namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IAuthService : IBaseService
{
    AuthorizationCodeDto ExternalLogin(Guid networkId, LoginDto loginData, string clientId, string codeChallenge);
    TokensDto Refresh(Guid networkId, RefreshTokenDto refreshToken, bool isExternal = true);
    AuthorizationCodeDto ExternalRegister(Guid networkId, CreateUserProxyDto createUserProxy, string clientId, string codeChallenge);
    TokensDto Token(TokenRequestDto dto);
    TokensDto MainNetworkLogin(LoginDto login);
    TokensDto MainNetworkRefresh(RefreshTokenDto refreshToken);
    TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy);
}
