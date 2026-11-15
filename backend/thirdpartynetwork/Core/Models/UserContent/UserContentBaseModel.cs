using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class UserContentBaseModel : BaseModel
{
    public Guid Id { get; set; }
    public Guid? AuthorUserProxyId { get; set; }
    [JsonIgnore]
    public UserProxy? AuthorUserProxy { get; set; }
    public Guid NetworkId { get; set; }
    [JsonIgnore]
    public Network Network { get; set; }
    public DateTime? PublishedAt { get; set; }
    public OwnershipMode OwnershipMode { get; set; } = OwnershipMode.AuthorOwned;
    public DateTime? DeletedAt { get; set; }
}
