namespace thirdpartynetwork.Application.Dtos.Network
{
    public class NetworkDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }
        public FileDto? ImageFile { get; set; }
        public DateTime CreatedOn { get; set; }
        public required bool IsPublic { get; init; }
        public ICollection<NetworkAccessNtADto> NetworkAccesses { get; set; } = [];
        public ICollection<RoleLightDto> Roles { get; set; } = [];
        public ICollection<NetworkUserLightUserInfoDto> NetworkUsers { get; set; } = [];
        public NetworkEntitlementDto? Entitlement { get; set; }
        public required bool IsSystemProtected { get; set; }
    }
    
    public class NetworkLightDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }
        public FileDto? ImageFile { get; set; }
        public DateTime CreatedOn { get; set; }
        public required bool IsPublic { get; init; }
        public NetworkEntitlementDto? Entitlement { get; set; }
        public required bool IsSystemProtected { get; set; }
    }
}
