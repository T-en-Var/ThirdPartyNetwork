namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IAuthService : IBaseService
{
    AuthorizationCodeDto Login(Guid networkId, LoginDto loginData, string clientId, string redirectUri, string codeChallenge);
    TokensDto Refresh(Guid networkId, RefreshTokenDto refreshToken);
    AuthorizationCodeDto Register(Guid networkId, CreateUserProxyDto createUserProxy, string clientId, string redirectUri, string codeChallenge);
    TokensDto Token(TokenRequestDto dto);
    TokensDto MainNetworkLogin(LoginDto login);
    TokensDto MainNetworkRefresh(RefreshTokenDto refreshToken);
    TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy);
}
