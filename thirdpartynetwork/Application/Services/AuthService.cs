using System.Security.Claims;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;

namespace thirdpartynetwork.Application.Services;

public class AuthService(ITokenGenerator tokenGenerator, IPasswordHasher passwordHasher, ILogger<AuthService> logger, IUserProxyService userProxyService, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public TokensDto Login(LoginDto login)
    {
        var user = userProxyService.GetByUsername(login.Username);
        if (user == null) throw new UnauthorizedHttpException("Invalid username or password");
        
        if (!passwordHasher.VerifyPassword(login.Password, user.Password)) throw new UnauthorizedHttpException("Invalid username or password");
        
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id, 
            login.Username);
        var refreshToken = tokenGenerator.GenerateRefreshToken();
        
        var tokenDto = new TokensDto()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return tokenDto;
    }

    public AccessTokenDto TokenRefresh(RefreshTokenDto refreshToken)
    {
        if (httpContextAccessor.HttpContext == null) throw new UnauthorizedHttpException();
        var user = httpContextAccessor.HttpContext.User;
        
        if (user == null) throw new UnauthorizedHttpException("No HttpContext User");
        
        logger.LogError(user.ToString());
        foreach (var claim in user.Claims)
        {
            logger.LogError(claim.ToString());
        }
        // logger.LogError();

        return new AccessTokenDto
        {
            AccessToken = "",
            // RefreshToken = user.ToString() ?? string.Empty,
        };
    }

    public TokensDto TokenRotate(RefreshTokenDto refreshToken)
    {
        throw new NotImplementedException();
    }

    public void Logout(RefreshTokenDto refreshToken)
    {
        throw new NotImplementedException();
    }
}