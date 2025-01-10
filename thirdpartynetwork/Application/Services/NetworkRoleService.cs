namespace thirdpartynetwork.Application.Services
{
    public class NetworkRoleService : INetworkRoleService
    {
        public ICollection<NetworkRoleDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkRoleDto> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkRoleDto> GetByRoleId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkRoleDto? GetByIds(Guid networkId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        public NetworkRoleDto? Create(Guid networkId, CreateNetworkRoleDto networkRole)
        {
            throw new NotImplementedException();
        }

        public NetworkRoleDto? Update(Guid networkId, Guid roleId, UpdateNetworkRoleDto networkRole)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByIds(Guid networkId, Guid roleId)
        {
            throw new NotImplementedException();
        }
    }
}
