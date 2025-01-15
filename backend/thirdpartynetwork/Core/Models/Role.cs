namespace thirdpartynetwork.Core.Models;

public class Role
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public Guid NetworkId { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    public Network Network { get; set; }
    public bool IsSystemProtected { get; set; } = false;
}
