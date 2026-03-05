using System.Text.Json;
using System.Text.Json.Nodes;

namespace thirdpartynetwork.Core.Models
{
    public class NetworkUser : BaseModel
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }
        public Guid UserProxyId { get; set; }
        public UserProxy UserProxy { get; set; }
        public Network Network { get; set; }
        public bool AccessIncomplete { get; set; } = false;
        public required JsonDocument Entitlements { get; set; } = JsonDocument.Parse("{}");
        public ICollection<NetworkUserAccess> NetworkUserAccesses { get; set; } = new List<NetworkUserAccess>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
        public ICollection<File> Files { get; set; } = new List<File>();
    }
}
