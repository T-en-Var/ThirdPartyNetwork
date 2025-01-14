namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IRoleVerifier
{
    bool VerifyHasRole(Guid networkId, Guid userProxyId, string roleName);
    bool IsSuperAdmin(Guid userProxyId);
}