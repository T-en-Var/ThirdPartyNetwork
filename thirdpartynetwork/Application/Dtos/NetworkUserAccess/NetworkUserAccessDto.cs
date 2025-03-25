using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Application.Dtos.Access;

namespace thirdpartynetwork.Application.Dtos.NetworkUserAccess
{
    public class NetworkUserAccessDto
    {
        public required bool IsAccepted { get; set; }
        public required NetworkUserDto NetworkUser { get; set; }
        public required AccessDto Access { get; set; }
    }
}
