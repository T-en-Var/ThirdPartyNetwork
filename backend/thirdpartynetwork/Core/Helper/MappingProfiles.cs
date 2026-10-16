using System.Text.Json;
using AutoMapper;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core.Interfaces.Auth;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Helper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Network, NetworkDto>();
        CreateMap<Network, NetworkLightDto>();
        CreateMap<CreateNetworkDto, Network>();
        CreateMap<UpdateNetworkDto, Network>()
            .ForMember(dest => dest.ExternalContentKinds, opt => opt.Condition(src => src.ExternalContentKinds.HasValue));

        CreateMap<NetworkAccess, NetworkAccessDto>();
        CreateMap<NetworkAccess, NetworkAccessNtADto>();
        CreateMap<NetworkAccess, NetworkAccessAtNDto>();
        CreateMap<CreateNetworkAccessDto, NetworkAccess>();
        CreateMap<UpdateNetworkAccessDto, NetworkAccess>();


        CreateMap<NetworkUser, NetworkUserDto>().ForMember(dest => dest.FeatureFlags,
            opt => opt.MapFrom(src => src.FeatureFlags.RootElement.Clone()));
        CreateMap<NetworkUser, NetworkUserLightDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<NetworkUser, NetworkUserLightWithNetworkDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<NetworkUser, NetworkUserLightUserInfoDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<NetworkUser, NetworkUserLightWithNetworkDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<CreateNetworkUserDto, NetworkUser>();
        CreateMap<UpdateNetworkUserDto, NetworkUser>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.FeatureFlags.GetRawText(), default)
            ));

        CreateMap<NetworkUserAccess, NetworkUserAccessDto>();
        CreateMap<NetworkUserAccess, NetworkUserAccessNtADto>();
        CreateMap<NetworkUserAccess, NetworkUserAccessAtNDto>();
        CreateMap<CreateNetworkUserAccessDto, NetworkUserAccess>();
        CreateMap<UpdateNetworkUserAccessDto, NetworkUserAccess>();

        CreateMap<NetworkUserRole, NetworkUserRoleDto>();
        CreateMap<NetworkUserRole, NetworkUserRoleNtRDto>();
        CreateMap<NetworkUserRole, NetworkUserRoleRtNDto>();
        CreateMap<CreateNetworkUserRoleDto, NetworkUserRole>();
        CreateMap<UpdateNetworkUserRoleDto, NetworkUserRole>();

        CreateMap<Access, AccessDto>();
        CreateMap<Access, AccessLightDto>();
        CreateMap<CreateAccessDto, Access>();
        CreateMap<UpdateAccessDto, Access>();

        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<Role, RoleLightDto>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                src.FeatureFlags.RootElement.Clone()
            ));
        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.FeatureFlags.GetRawText(), default)
            ));
        CreateMap<UpdateRoleDto, Role>()
            .ForMember(dest => dest.FeatureFlags, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.FeatureFlags.GetRawText(), default)
            ));

        CreateMap<RolePermission, RolePermissionDto>();
        CreateMap<RolePermission, RolePermissionRtPDto>();
        CreateMap<RolePermission, RolePermissionPtRDto>();
        CreateMap<CreateRolePermissionDto, RolePermission>();
        CreateMap<UpdateRolePermissionDto, RolePermission>();

        CreateMap<Permission, PermissionDto>();
        CreateMap<Permission, PermissionLightDto>();
        CreateMap<CreatePermissionDto, Permission>();
        CreateMap<UpdatePermissionDto, Permission>();

        CreateMap<User, UserDto>();
        CreateMap<User, UserLightDto>();
        CreateMap<User, UserWithNetworksDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        CreateMap<UserProxy, UserProxyDto>()
            .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => src.Password != null))
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessEmail, s => s.Email)))
            .ForMember(dest => dest.FirstName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessFirstname, s => s.FirstName)))
            .ForMember(dest => dest.LastName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessLastname, s => s.LastName)))
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessUsername, s => s.Username)))
            .ForMember(dest => dest.ImageFile, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessProfilePicture,
                        s => s.ImageFile)));
        CreateMap<UserProxy, UserProxyLightDto>()
            .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => src.Password != null))
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessEmail, s => s.Email)))
            .ForMember(dest => dest.FirstName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessFirstname, s => s.FirstName)))
            .ForMember(dest => dest.LastName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessLastname, s => s.LastName)))
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessUsername, s => s.Username)))
            .ForMember(dest => dest.ImageFile, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessProfilePicture,
                        s => s.ImageFile)));
        CreateMap<UserProxy, UserProxyLightWithNetworksDto>()
            .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => src.Password != null))
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessEmail, s => s.Email)))
            .ForMember(dest => dest.FirstName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessFirstname, s => s.FirstName)))
            .ForMember(dest => dest.LastName, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessLastname, s => s.LastName)))
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessUsername, s => s.Username)))
            .ForMember(dest => dest.ImageFile, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessProfilePicture,
                        s => s.ImageFile)));
        CreateMap<UserProxy, UserProxyLightShortProfileDto>()
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom((src, _, _, context) => MapFieldIfAccessGranted(src, context, Constants.Access.AccessUsername, s => s.Username)))
            .ForMember(dest => dest.ImageFile, opt =>
                opt.MapFrom((src, _, _, context) =>
                    MapFieldIfAccessGranted(src, context, Constants.Access.AccessProfilePicture,
                        s => s.ImageFile)));
        CreateMap<UserProxy, UserProxyLightNoCredentialsDto>();
        CreateMap<CreateUserProxyDto, UserProxy>();
        CreateMap<UpdateUserProxyDto, UserProxy>();
        CreateMap<AddUserProxyDto, UserProxy>();

        CreateMap<File, FileDto>()
            .ForMember(dest => dest.ViewingUrl, opt => opt.MapFrom<FileViewingUrlResolver>());
        CreateMap<File, FileLightDto>();
        CreateMap<CreateFileDto, File>();
        CreateMap<UpdateFileDto, File>();

        CreateMap<CustomPage, CustomPageDto>();
        CreateMap<CustomPage, CustomPageLightDto>();
        CreateMap<CreateCustomPageDto, CustomPage>();
        CreateMap<UpdateCustomPageDto, CustomPage>();

        CreateMap<PageBlock, PageBlockDto>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                src.Data.RootElement.Clone()
            ));
        CreateMap<PageBlock, PageBlockLightDto>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                src.Data.RootElement.Clone()
            ));
        CreateMap<CreatePageBlockDto, PageBlock>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.Data.GetRawText(), default)
            ));
        CreateMap<UpdatePageBlockDto, PageBlock>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.Data.GetRawText(), default)
            ));

        CreateMap<Blog, BlogDto>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src =>
                src.Body.RootElement.Clone()
            ));
        CreateMap<Blog, BlogLightDto>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src =>
                src.Body.RootElement.Clone()
            ));
        CreateMap<CreateBlogDto, Blog>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.Body.GetRawText(), default)
            ));
        CreateMap<UpdateBlogDto, Blog>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.Body.GetRawText(), default)
            ));

        CreateMap<Configuration, ConfigurationDto>();
        CreateMap<Configuration, ConfigurationLightDto>();
        CreateMap<CreateConfigurationDto, Configuration>();
        CreateMap<UpdateConfigurationDto, Configuration>();

        CreateMap<ContentAccessGrant, ContentAccessGrantDto>();
        CreateMap<ContentModerationFlag, ContentModerationFlagDto>();

        CreateMap<NetworkFeatureFlag, NetworkFeatureFlagDto>();
        CreateMap<CreateNetworkFeatureFlagDto, NetworkFeatureFlag>();
        CreateMap<UpdateNetworkFeatureFlagDto, NetworkFeatureFlag>();
    }

    private T? MapFieldIfAccessGranted<T>(
        UserProxy src,
        ResolutionContext context,
        string accessName,
        Func<UserProxy, T?> fieldSelector)
    {
        if (context.TryGetItems(out var i) &&
            i.TryGetValue("bypass", out var bypassObj) &&
            bypassObj is true)
        {
            return fieldSelector(src);
        }
        
        if (!context.TryGetItems(out var items) ||
            !items.TryGetValue("networkId", out var networkIdObj) ||
            networkIdObj is not Guid networkId)
        {
            return default;
        }
        
        // Console.WriteLine($"NetworkId: {networkId}");

        var access = src.NetworkUsers
            .FirstOrDefault(nu => nu.NetworkId == networkId)?
            .NetworkUserAccesses
            .FirstOrDefault(a => a.Access.Name == accessName);

        // Console.WriteLine($"{src.Username} | {fieldSelector(src)}: {accessName} - IsAccepted: {access?.IsAccepted}\n");

        return access?.IsAccepted == true ? fieldSelector(src) : default;
    }
}