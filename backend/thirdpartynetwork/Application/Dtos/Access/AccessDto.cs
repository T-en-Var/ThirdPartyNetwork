using System.Text.Json.Serialization;

namespace thirdpartynetwork.Application.Dtos.Access
{
    public class AccessDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        [JsonIgnore]
        public ICollection<NetworkAccessAtNDto> NetworkAccesses { get; set; } = [];
        [JsonIgnore]
        public ICollection<NetworkUserAccessAtNDto> NetworkUserAccesses { get; set; } = [];
    }
    
    public class AccessLightDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
