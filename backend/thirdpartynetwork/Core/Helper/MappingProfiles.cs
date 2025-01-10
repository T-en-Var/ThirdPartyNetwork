using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos;

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
