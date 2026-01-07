using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;

public class CustomUserContentBaseDto
{
    public Guid NetworkId { get; set; }
    public Guid AuthorId { get; set; }
    public required NetworkUserDto Author { get; set; }
    public required AccessLevel AccessLevel { get; set; }
}