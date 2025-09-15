using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

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
                .ForMember(dest => dest.RolePermissions, opt => opt.MapFrom(src => src.RolePermissions));
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<RolePermission, RolePermissionDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Permission, opt => opt.MapFrom(src => src.Permission));
            CreateMap<CreateRolePermissionDto, RolePermission>();
            CreateMap<UpdateRolePermissionDto, RolePermission>();

            CreateMap<Permission, PermissionDto>()
                .ForMember(dest => dest.RolePermissions, opt => opt.MapFrom(src => src.RolePermissions));
            CreateMap<CreatePermissionDto, Permission>();
            CreateMap<UpdatePermissionDto, Permission>();

            CreateMap<User, UserDto>()
                .ForMember(u => u.UserProxies, opt => opt.MapFrom(u => u.UserProxies));
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<UserProxy, UserProxyDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.NetworkUsers, opt => opt.MapFrom(src => src.NetworkUsers))
                .ForMember(dest => dest.Email, opt =>
                    opt.MapFrom((src, dest, destMember, context) =>
                        MapFieldIfAccessGranted(src, context, Constants.Access.AccessEmail, s => s.Email)))
                .ForMember(dest => dest.FirstName, opt =>
                    opt.MapFrom((src, dest, destMember, context) =>
                        MapFieldIfAccessGranted(src, context, Constants.Access.AccessFirstname, s => s.FirstName)))
                .ForMember(dest => dest.LastName, opt =>
                    opt.MapFrom((src, dest, destMember, context) =>
                        MapFieldIfAccessGranted(src, context, Constants.Access.AccessLastname, s => s.LastName)))
                .ForMember(dest => dest.Username, opt =>
                    opt.MapFrom((src, dest, destMember, context) =>
                        MapFieldIfAccessGranted(src, context, Constants.Access.AccessUsername, s => s.Username)))
                .ForMember(dest => dest.ImageFile, opt =>
                    opt.MapFrom((src, dest, destMember, context) =>
                        MapFieldIfAccessGranted(src, context, Constants.Access.AccessProfilePicture, s => s.ImageFile)));
            CreateMap<CreateUserProxyDto, UserProxy>();
            CreateMap<UpdateUserProxyDto, UserProxy>();

            CreateMap<File, FileDto>();
                // .ForMember(dest => dest.Author.Network, opt => opt.Ignore());
            CreateMap<CreateFileDto, File>();
            CreateMap<UpdateFileDto, File>();
        }
        
        private static T? MapFieldIfAccessGranted<T>(
            UserProxy src,
            ResolutionContext context,
            string accessName,
            Func<UserProxy, T?> fieldSelector)
        {
            if (!context.TryGetItems(out var items) ||
                !items.TryGetValue("networkId", out var networkIdObj) ||
                networkIdObj is not Guid networkId)
            {
                return fieldSelector(src);
            }

            var access = src.NetworkUsers
                .FirstOrDefault(nu => nu.NetworkId == networkId)?
                .NetworkUserAccesses
                .FirstOrDefault(a => a.Access.Name == accessName);

            return access?.IsAccepted == true ? fieldSelector(src) : default;
        }
    }
}
