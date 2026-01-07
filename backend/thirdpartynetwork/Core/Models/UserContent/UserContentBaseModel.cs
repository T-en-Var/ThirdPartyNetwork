using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models;

public class UserContentBaseModel : BaseModel
{
    public Guid AuthorId { get; set; }
    public NetworkUser Author { get; set; }
    public Guid NetworkId { get; set; }
    public Network Network { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
}