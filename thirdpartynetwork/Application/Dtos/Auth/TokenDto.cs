namespace thirdpartynetwork.Application.Dtos.Auth;

public class TokenDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}