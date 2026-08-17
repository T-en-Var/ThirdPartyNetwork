namespace thirdpartynetwork.Application.Dtos.ContentModerationFlag;

public class CreateContentModerationFlagDto
{
    public required string Reason { get; set; }

    // Flagging defaults to hiding — a moderator raising a flag is typically also acting on
    // it immediately; set false to just log a report without suppressing anything yet.
    public bool HiddenInNetwork { get; set; } = true;
}
