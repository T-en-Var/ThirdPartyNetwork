using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class ContentModerationFlag : BaseModel
{
    public Guid Id { get; set; }

    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }

    public Guid NetworkId { get; set; }
    [JsonIgnore]
    public Network Network { get; set; }

    public Guid? FlaggedByNetworkUserId { get; set; }
    [JsonIgnore]
    public NetworkUser? FlaggedByNetworkUser { get; set; }

    public string Reason { get; set; } = "";
    public ModerationFlagStatus Status { get; set; } = ModerationFlagStatus.Open;
    public bool HiddenInNetwork { get; set; }
}
