namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class NetworkUserDto
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public required UserProxyDto UserProxy { get; set; }
        public required NetworkDto Network { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<NetworkUserAccessDto> NetworkUserAccesses { get; set; } = [];
        public ICollection<NetworkUserRoleDto> NetworkUserRoles { get; set; } = [];
    }
}
