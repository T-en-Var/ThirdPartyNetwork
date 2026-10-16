using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class ContentAccessGrant : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    public GranteeType GranteeType { get; set; }

    public Guid? GranteeNetworkId { get; set; }
    [JsonIgnore]
    public Network? GranteeNetwork { get; set; }

    public Guid? GranteeRoleId { get; set; }
    [JsonIgnore]
    public Role? GranteeRole { get; set; }

    public Guid? GranteePermissionId { get; set; }
    [JsonIgnore]
    public Permission? GranteePermission { get; set; }

    public Guid? GranteeUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? GranteeUserProxy { get; set; }

    public ContentCapabilities Capabilities { get; set; } = ContentCapabilities.View;

    public Guid? GrantedByNetworkUserId { get; set; }
    [JsonIgnore]
    public NetworkUser? GrantedByNetworkUser { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
