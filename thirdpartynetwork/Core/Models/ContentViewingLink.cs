using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class ContentViewingLink : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    public Guid? IssuedForUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? IssuedForUserProxy { get; set; }

    public Guid? IssuedForNetworkId { get; set; }
    [JsonIgnore]
    public Network? IssuedForNetwork { get; set; }

    public ContentViewingLinkMode Mode { get; set; } = ContentViewingLinkMode.OneTimeView;

    public DateTime? ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
