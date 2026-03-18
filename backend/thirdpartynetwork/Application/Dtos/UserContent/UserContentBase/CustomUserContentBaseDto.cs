using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.UserContentBase
{
    public class CustomUserContentBaseDto
    {
        public Guid NetworkId { get; set; }
        public Guid AuthorId { get; set; }
        public NetworkUserLightDto Author { get; set; }
        public required AccessLevel AccessLevel { get; set; }
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class CustomUserContentBaseLightDto
    {
        public required AccessLevel AccessLevel { get; set; }
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}