using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.ContentModerationFlag;

public class ContentModerationFlagDto
{
    public Guid Id { get; set; }
    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }
    public Guid NetworkId { get; set; }
    public Guid? FlaggedByNetworkUserId { get; set; }
    public string Reason { get; set; } = "";
    public ModerationFlagStatus Status { get; set; }
    public bool HiddenInNetwork { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedAt { get; set; }
}
