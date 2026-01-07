using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage;

public class CreateCustomPageDto : CreateUserContentBaseDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required AccessLevel AccessLevel { get; set; }
}