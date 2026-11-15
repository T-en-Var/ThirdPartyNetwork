using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.CustomPage
{
    public class CustomPageDto : UserContentBaseDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public ICollection<PageBlockLightDto> Blocks { get; set; }
    }
    
    public class CustomPageLightDto : UserContentBaseLightDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
    }
}