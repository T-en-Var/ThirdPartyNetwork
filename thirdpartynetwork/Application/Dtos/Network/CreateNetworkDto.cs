namespace thirdpartynetwork.Application.Dtos.Network
{
    public class CreateNetworkDto
    {
        public required string Name { get; init; }
        public required bool IsPublic { get; init; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
