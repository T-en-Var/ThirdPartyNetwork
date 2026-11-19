using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models
{
    public class NetworkUserAccess : BaseModel
    {
        public Guid NetworkUserId { get; set; }
        public Guid AccessId { get; set; }
        public required bool IsAccepted { get; set; }
        [JsonIgnore]
        public NetworkUser NetworkUser { get; set; }
        public Access Access { get; set; }
    }
}
