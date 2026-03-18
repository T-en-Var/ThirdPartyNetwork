using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models
{
    public class NetworkAccess : BaseModel
    {
        public Guid NetworkId { get; set; }
        public Guid AccessId { get; set; }
        public required bool IsRequired { get; set; }
        [JsonIgnore]
        public Network Network { get; set; }
        public required Access Access { get; set; }
        public bool IsSystemProtected { get; set; } = false;
    }
}
