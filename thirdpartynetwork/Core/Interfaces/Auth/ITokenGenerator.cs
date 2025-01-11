namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId, string username);
    string GenerateRefreshToken(string? previousJti = null);
}
