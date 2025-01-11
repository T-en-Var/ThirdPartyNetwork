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
            
            CreateMap<NetworkPermission, NetworkPermissionDto>();
            CreateMap<CreateNetworkPermissionDto, NetworkPermission>();
            CreateMap<UpdateNetworkPermissionDto, NetworkPermission>();
            
            CreateMap<NetworkRole, NetworkRoleDto>();
            CreateMap<CreateNetworkRoleDto, NetworkRole>();
            CreateMap<UpdateNetworkRoleDto, NetworkRole>();
            
            CreateMap<NetworkUser, NetworkUserDto>();
            CreateMap<CreateNetworkUserDto, NetworkUser>();
            CreateMap<UpdateNetworkUserDto, NetworkUser>();
            
            CreateMap<NetworkUserPermission, NetworkUserPermissionDto>();
            CreateMap<CreateNetworkUserPermissionDto, NetworkUserPermission>();
            CreateMap<UpdateNetworkUserPermissionDto, NetworkUserPermission>();
            
            CreateMap<NetworkUserRole, NetworkUserRoleDto>();
            CreateMap<CreateNetworkUserRoleDto, NetworkUserRole>();
            CreateMap<UpdateNetworkUserRoleDto, NetworkUserRole>();

            CreateMap<Permission, PermissionDto>();
            CreateMap<CreatePermissionDto, Permission>();
            CreateMap<UpdatePermissionDto, Permission>();

            CreateMap<Role, RoleDto>();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            
            CreateMap<UserProxy, UserProxyDto>();
            CreateMap<CreateUserProxyDto, UserProxy>();
            CreateMap<UpdateUserProxyDto, UserProxy>();
        }
    }
}
