namespace thirdpartynetwork.Application.Dtos.Network
{
    public class NetworkDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkPermissionDto> NetworkPermissions { get; set; } = [];
        public ICollection<NetworkRoleDto> NetworkRoles { get; set; } = [];
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
    }
}
