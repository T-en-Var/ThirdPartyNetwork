using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.ContentModerationFlag;

public class ResolveContentModerationFlagDto
{
    public required ModerationFlagStatus Status { get; set; }
    public bool? HiddenInNetwork { get; set; }
}
