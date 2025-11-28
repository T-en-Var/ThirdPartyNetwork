namespace thirdpartynetwork.Application.Dtos.Auth;

public class TokenRequestDto
{
    public string Code { get; set; }
    public string CodeVerifier { get; set; }
    public string ClientId { get; set; }
}