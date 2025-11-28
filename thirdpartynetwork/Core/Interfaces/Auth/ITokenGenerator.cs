using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface ITokenGenerator
{
    // TokensDto GenerateTokenPair(NetworkUser networkUser, string? previousJti = null);
    // string GenerateAccessToken(Guid userId, string username);
    // string GenerateRefreshToken(string? previousJti = null);
    TokensDto GenerateTokenPair(
        NetworkUser networkUser,
        string? audience = null, // New optional parameter for dynamic audience
        Guid? networkId = null,  // New optional parameter for claim filtering
        string? previousJti = null
    );
}
