using AutoMapper;
using thirdpartynetwork.Dtos;
using thirdpartynetwork.Dtos.Network;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Helper
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
