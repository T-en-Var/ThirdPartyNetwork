using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models;

public class Permission : BaseModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    [JsonIgnore]
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public Network? Network { get; set; }
    public bool IsSystemProtected { get; set; } = false;
}
