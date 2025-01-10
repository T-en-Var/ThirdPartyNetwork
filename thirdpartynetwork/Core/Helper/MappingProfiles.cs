using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Dtos;

namespace thirdpartynetwork.Core.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Network, NetworkDto>();
            CreateMap<CreateNetworkDto, Network>();
        }
    }
}
