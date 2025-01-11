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
            
            CreateMap<User, UserDto>()
                .ForMember(usr => usr.UserProxies, opt => opt.MapFrom(src => src.Proxies));
            CreateMap<CreateUserDto, User>();
            
            CreateMap<UserProxy, UserProxyDto>();
            CreateMap<CreateUserProxyDto, UserProxy>();
        }
    }
}
