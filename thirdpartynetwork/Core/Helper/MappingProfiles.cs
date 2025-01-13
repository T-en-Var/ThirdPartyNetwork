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
            
            CreateMap<NetworkPermission, NetworkPermissionDto>()
                .ForMember(n => n.Permission, opt => opt.MapFrom(n => n.Permission))
                .ForMember(n => n.Network, opt => opt.MapFrom(n => n.Network));
            CreateMap<CreateNetworkPermissionDto, NetworkPermission>();
            CreateMap<UpdateNetworkPermissionDto, NetworkPermission>();
            
            CreateMap<NetworkRole, NetworkRoleDto>()
                .ForMember(n => n.Role, opt => opt.MapFrom(n => n.Role))
                .ForMember(n => n.Network, opt => opt.MapFrom(n => n.Network));
            CreateMap<CreateNetworkRoleDto, NetworkRole>();
            CreateMap<UpdateNetworkRoleDto, NetworkRole>();
            
            CreateMap<NetworkUser, NetworkUserDto>()
                .ForMember(n => n.UserProxy, opt => opt.MapFrom(u => u.UserProxy));
            CreateMap<CreateNetworkUserDto, NetworkUser>();
            CreateMap<UpdateNetworkUserDto, NetworkUser>();
            
            CreateMap<NetworkUserPermission, NetworkUserPermissionDto>()
                .ForMember(dest => dest.Permission, opt => opt.MapFrom(src => src.Permission))
                .ForMember(dest => dest.NetworkUser, opt => opt.MapFrom(src => src.NetworkUser));
            CreateMap<CreateNetworkUserPermissionDto, NetworkUserPermission>();
            CreateMap<UpdateNetworkUserPermissionDto, NetworkUserPermission>();

            CreateMap<NetworkUserRole, NetworkUserRoleDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.NetworkUser, opt => opt.MapFrom(src => src.NetworkUser));
            CreateMap<CreateNetworkUserRoleDto, NetworkUserRole>();
            CreateMap<UpdateNetworkUserRoleDto, NetworkUserRole>();

            CreateMap<Permission, PermissionDto>();
            CreateMap<CreatePermissionDto, Permission>();
            CreateMap<UpdatePermissionDto, Permission>();

            CreateMap<Role, RoleDto>();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

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
