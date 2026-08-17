using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.ContentModerationFlag;

public class ResolveContentModerationFlagDto
{
    public required ModerationFlagStatus Status { get; set; }

    // Orthogonal to Status (§2.5) — HiddenInNetwork is its own toggle, not implied by
    // Resolved/Dismissed. Null leaves the current value alone.
    public bool? HiddenInNetwork { get; set; }
}
