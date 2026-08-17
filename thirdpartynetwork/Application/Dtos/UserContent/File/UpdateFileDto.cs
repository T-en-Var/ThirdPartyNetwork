namespace thirdpartynetwork.Application.Dtos.UserContent.File
{
    public class UpdateFileDto
    {
        public required string Name { get; set; }
        // AccessLevel removed (§2.1) — visibility now moves only through the
        // publish/unpublish actions and the ContentAccessGrant endpoints (§6), never a
        // directly-settable field on the content update DTO itself.
    }
}
