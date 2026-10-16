using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.UserContentBase
{
    public class UserContentBaseDto
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }
        public Guid? AuthorUserProxyId { get; set; }
        public UserProxyLightShortProfileDto? AuthorUserProxy { get; set; }

        public DateTime? PublishedAt { get; set; }
        public OwnershipMode OwnershipMode { get; set; }
        public string? ViewingUrl { get; set; }

        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserContentBaseLightDto
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
