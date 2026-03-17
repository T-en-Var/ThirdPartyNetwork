using System.Text.Json.Serialization;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class UserContentBaseModel : BaseModel
{
    public Guid AuthorId { get; set; }
    [JsonIgnore]
    public NetworkUser Author { get; set; }
    public Guid NetworkId { get; set; }
    [JsonIgnore]
    public Network Network { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
}