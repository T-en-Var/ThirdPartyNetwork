using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage
{
    public class PageDto : UserContentBaseDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public ICollection<PageBlockLightDto> Pages { get; set; }
    }
    
    public class PageLightDto : UserContentBaseLightDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
    }
}