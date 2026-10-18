namespace thirdpartynetwork.Application.Dtos.ContentModerationFlag;

public class CreateContentModerationFlagDto
{
    public required string Reason { get; set; }
    public bool HiddenInNetwork { get; set; } = true;
}
