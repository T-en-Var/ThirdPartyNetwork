using System.Text.Json.Serialization;

namespace thirdpartynetwork.Application.Dtos.NetworkAccess
{
    public class NetworkAccessDto
    {
        public Guid NetworkId { get; set; }
        public Guid AccessId { get; set; }
        public required bool IsRequired { get; set; }
        public required AccessLightDto Access { get; set; }
        public required NetworkLightDto Network { get; set; }
    }
    public class NetworkAccessNtADto
    {
        public Guid AccessId { get; set; }
        public required bool IsRequired { get; set; }
        public required AccessLightDto Access { get; set; }
    }
    public class NetworkAccessAtNDto
    {
        public Guid NetworkId { get; set; }
        public required bool IsRequired { get; set; }
        public required NetworkLightDto Network { get; set; }
    }
}
