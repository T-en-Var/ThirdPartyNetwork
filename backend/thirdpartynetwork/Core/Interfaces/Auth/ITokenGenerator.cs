namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface ITokenGenerator
{
    TokensDto GenerateTokenPair(Guid userId, string username, string? previousJti = null);
    // string GenerateAccessToken(Guid userId, string username);
    // string GenerateRefreshToken(string? previousJti = null);
}
