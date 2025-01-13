namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IAuthService : IBaseService
{
    TokensDto Login(LoginDto login);
    AccessTokenDto TokenRefresh(RefreshTokenDto refreshToken);
    TokensDto TokenRotate(RefreshTokenDto refreshToken);
    void Logout(RefreshTokenDto refreshToken);
}
