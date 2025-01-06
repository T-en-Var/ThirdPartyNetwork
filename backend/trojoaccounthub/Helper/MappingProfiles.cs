using AutoMapper;
using trojoaccounthub.Dtos;
using trojoaccounthub.Models;

namespace trojoaccounthub.Helper
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
