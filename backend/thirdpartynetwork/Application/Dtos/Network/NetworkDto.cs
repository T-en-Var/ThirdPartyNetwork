namespace thirdpartynetwork.Application.Dtos.Network
{
    public class NetworkDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkAccessDto> NetworkAccesses { get; set; } = [];
        public ICollection<RoleDto> Roles { get; set; } = [];
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public required bool IsSystemProtected { get; set; }
    }
}
