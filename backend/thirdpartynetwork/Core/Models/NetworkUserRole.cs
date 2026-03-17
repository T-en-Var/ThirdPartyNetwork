using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models
{
    public class NetworkUserRole : BaseModel
    {
        public Guid NetworkUserId { get; set; }
        public Guid RoleId { get; set; }
        [JsonIgnore]
        public NetworkUser NetworkUser { get; set; }
        public Role Role { get; set; }
    }
}
