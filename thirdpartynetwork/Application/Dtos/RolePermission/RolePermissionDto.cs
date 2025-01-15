namespace thirdpartynetwork.Application.Dtos.RolePermission;

public class RolePermissionDto
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public RoleDto Role { get; set; }
    public PermissionDto Permission { get; set; }
}