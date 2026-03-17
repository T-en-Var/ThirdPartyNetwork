using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models;

public class RolePermission : BaseModel
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    [JsonIgnore]
    public Role Role { get; set; }
    public required Permission Permission { get; set; }
}
