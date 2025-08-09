using thirdpartynetwork.Application.Dtos.NetworkAccess;
using thirdpartynetwork.Application.Dtos.NetworkUserAccess;

namespace thirdpartynetwork.Application.Dtos.Access
{
    public class AccessDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<NetworkAccessDto> NetworkAccesses { get; set; } = [];
        public ICollection<NetworkUserAccessDto> NetworkUserAccesses { get; set; } = [];
    }
}
