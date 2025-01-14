using AutoMapper;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Content, ContentDto>();
            CreateMap<CreateContentDto, Content>();
            CreateMap<UpdateContentDto, Content>();
            
            CreateMap<Network, NetworkDto>();
            CreateMap<CreateNetworkDto, Network>();
            CreateMap<UpdateNetworkDto, Network>();
            
            CreateMap<NetworkAccess, NetworkAccessDto>()
                .ForMember(n => n.Access, opt => opt.MapFrom(n => n.Access))
                .ForMember(n => n.Network, opt => opt.MapFrom(n => n.Network));
            CreateMap<CreateNetworkAccessDto, NetworkAccess>();
            CreateMap<UpdateNetworkAccessDto, NetworkAccess>();
            
            CreateMap<NetworkUser, NetworkUserDto>()
                .ForMember(n => n.UserProxy, opt => opt.MapFrom(u => u.UserProxy));
            CreateMap<CreateNetworkUserDto, NetworkUser>();
            CreateMap<UpdateNetworkUserDto, NetworkUser>();
            
            CreateMap<NetworkUserAccess, NetworkUserAccessDto>()
                .ForMember(dest => dest.Access, opt => opt.MapFrom(src => src.Access))
                .ForMember(dest => dest.NetworkUser, opt => opt.MapFrom(src => src.NetworkUser));
            CreateMap<CreateNetworkUserAccessDto, NetworkUserAccess>();
            CreateMap<UpdateNetworkUserAccessDto, NetworkUserAccess>();

            CreateMap<NetworkUserRole, NetworkUserRoleDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.NetworkUser, opt => opt.MapFrom(src => src.NetworkUser));
            CreateMap<CreateNetworkUserRoleDto, NetworkUserRole>();
            CreateMap<UpdateNetworkUserRoleDto, NetworkUserRole>();

            CreateMap<Access, AccessDto>();
            CreateMap<CreateAccessDto, Access>();
            CreateMap<UpdateAccessDto, Access>();

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.RoleRules, opt => opt.MapFrom(src => src.RoleRules));
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<RoleRule, RoleRuleDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Rule, opt => opt.MapFrom(src => src.Rule));
            CreateMap<CreateRoleRuleDto, RoleRule>();
            CreateMap<UpdateRoleRuleDto, RoleRule>();

            CreateMap<Rule, RuleDto>()
                .ForMember(dest => dest.RoleRules, opt => opt.MapFrom(src => src.RoleRules));
            CreateMap<CreateRuleDto, Rule>();
            CreateMap<UpdateRuleDto, Rule>();

            CreateMap<User, UserDto>()
                .ForMember(u => u.UserProxies, opt => opt.MapFrom(u => u.Proxies));
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            
            CreateMap<UserProxy, UserProxyDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
            CreateMap<CreateUserProxyDto, UserProxy>();
            CreateMap<UpdateUserProxyDto, UserProxy>();
        }
    }
}
