using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.UserContentBase
{
    public class UserContentBaseDto
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }

        // §2.1: Author is now a UserProxy (portable persona), not a NetworkUser membership
        // row — nullable since a hard-deleted persona SetNulls this (§2.1).
        public Guid? AuthorUserProxyId { get; set; }
        public UserProxyLightShortProfileDto? AuthorUserProxy { get; set; }

        public DateTime? PublishedAt { get; set; }
        public OwnershipMode OwnershipMode { get; set; }

        // AccessLevel is gone (§2.1) — replaced by ViewingUrl (§6): computed fresh per
        // request by the controller (not AutoMapper — minting a ContentViewingLink is
        // async), null until the controller assigns it.
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
