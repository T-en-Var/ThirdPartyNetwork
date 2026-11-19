namespace thirdpartynetwork.Application.Dtos.Auth;

public class TokensDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}