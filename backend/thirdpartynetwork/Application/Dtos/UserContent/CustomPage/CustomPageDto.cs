using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage;

public class CustomPageDto : CustomUserContentBaseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public ICollection<PageBlockDto> Pages { get; set; }
}