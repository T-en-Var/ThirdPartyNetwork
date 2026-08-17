using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage;

public class CreateCustomPageDto : CreateUserContentBaseDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    // AccessLevel removed (§2.1/§6).
}
