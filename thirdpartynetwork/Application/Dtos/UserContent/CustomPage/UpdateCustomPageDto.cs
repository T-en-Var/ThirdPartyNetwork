namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage;

public class UpdateCustomPageDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    // AccessLevel removed (§2.1/§6).
}
