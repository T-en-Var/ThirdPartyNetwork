namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IAuthService : IBaseService
{
    TokensDto Login(Guid networkId, LoginDto login);
    TokensDto Refresh(Guid networkId, RefreshTokenDto refreshToken);   
    TokensDto Register(Guid networkId, CreateUserProxyDto createUserProxy);   
    TokensDto MainNetworkLogin(LoginDto login);
    TokensDto MainNetworkRefresh(RefreshTokenDto refreshToken);
    TokensDto MainNetworkRegister(CreateUserProxyDto createUserProxy);
}
