namespace thirdpartynetwork.Application.Dtos.Network
{
    public class NetworkDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public required bool IsPublic { get; init; }
        public ICollection<NetworkAccessDto> NetworkAccesses { get; set; } = [];
        public ICollection<RoleDto> Roles { get; set; } = [];
        public ICollection<NetworkUserDto> NetworkUsers { get; set; } = [];
        public required bool IsSystemProtected { get; set; }
    }
}
