using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.Network
{
    public class UpdateNetworkDto
    {
        public required string Name { get; set; }
        public required bool IsPublic { get; init; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }
        public FileLinkDto? FileLink { get; set; }

        // PLAN.md §2.6/§6: network-wide policy knob, gated the same as the rest of this
        // DTO (ManageNetwork) — nullable so existing callers that don't know about it yet
        // don't have to resend the current value on every unrelated network edit.
        public ContentKindFlags? ExternalContentKinds { get; set; }
    }
}
