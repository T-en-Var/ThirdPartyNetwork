using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class RoleVerifier(INetworkService networkService) : IRoleVerifier
{
    public bool VerifyHasRole(Guid networkId, Guid userProxyId, string roleName)
    {
        var network = networkService.GetById(networkId);
        var networkUser = network?.NetworkUsers.FirstOrDefault(u => u.UserProxyId == userProxyId);
        var userRole = networkUser?.NetworkUserRoles.FirstOrDefault(r => r.Role.Name == roleName);
        return userRole != null;
    }

    public bool IsSuperAdmin(Guid userProxyId)
    {
        var tpn = networkService.GetMainNetwork();
        var networkUser = tpn.NetworkUsers.FirstOrDefault(u => u.Id == userProxyId);
        var userRole = networkUser?.NetworkUserRoles.FirstOrDefault(u => u.Role.Name == "Admin");
        return userRole != null;
    }
}
