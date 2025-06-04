namespace thirdpartynetwork.Core.Models;

// NOT stored in database, but in memory
public class AuthorizationCode
{
    public string Code { get; set; }
    public Guid UserId { get; set; }
    public Guid NetworkId { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string CodeChallenge { get; set; }
    public DateTime ExpiresAt { get; set; }
}