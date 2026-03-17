using System.Text.Json;
using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models;

public class Role : BaseModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool IsDefault { get; set; }
    public Guid NetworkId { get; set; }
    public required JsonDocument Entitlements { get; set; } = JsonDocument.Parse("{}");
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    [JsonIgnore]
    public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    [JsonIgnore]
    public Network Network { get; set; }
    public bool IsSystemProtected { get; set; } = false;
}
