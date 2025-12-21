using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage;

public class UpdateCustomPageDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required AccessLevel AccessLevel { get; set; }
}