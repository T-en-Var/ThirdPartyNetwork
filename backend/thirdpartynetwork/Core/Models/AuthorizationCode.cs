namespace thirdpartynetwork.Core.Models;

// NOT stored in database, but in memory
public class AuthorizationCode
{
    public required string Code { get; set; }
    public required Guid UserId { get; set; }
    public required Guid NetworkId { get; set; }
    public required string ClientId { get; set; }
    public required string CodeChallenge { get; set; }
    public required string Audience { get; set; }
    public required DateTime ExpiresAt { get; set; }
}