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
        public ContentKindFlags? ExternalContentKinds { get; set; }
    }
}
