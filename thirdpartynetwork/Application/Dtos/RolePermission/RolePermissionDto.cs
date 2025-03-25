namespace thirdpartynetwork.Application.Dtos.RolePermission;

public class RolePermissionDto
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public required RoleDto Role { get; set; }
    public required PermissionDto Permission { get; set; }
}