using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class MeService(IUserProxyService userProxyService, IIdentityService identityService, IRoleService roleService) : IMeService
    {
        
        
        public async Task<UserProxy?> GetMeAsync()
        {
            var userProxyId = identityService.GetUserProxyId();
            return await userProxyService.GetByIdAsync(userProxyId);
        }

        public async Task<ICollection<NetworkPermissionsCollectionDto>> GetMyPermissionsAsync()
        {
            var userProxyId = identityService.GetUserProxyId();
            var roles = await roleService.GetByUserProxyIdAsync(userProxyId);
            
            var collection = roles
                .GroupBy(role => new { role.NetworkId, NetworkName = role.Network.Name })
                .Select(networkGroup => new NetworkPermissionsCollectionDto
                {
                    Id = networkGroup.Key.NetworkId,
                    Name = networkGroup.Key.NetworkName,

                    Permissions = networkGroup
                        .SelectMany(role => role.RolePermissions).Select(rp => rp.Permission)
                        .DistinctBy(permission => permission.Id) 
                        .Select(permission => new PermissionShortDto
                        {
                            Id = permission.Id,
                            Name = permission.Name
                        })
                        .ToList()
                })
                .ToList();
            
            return collection;
        }
    }
}
