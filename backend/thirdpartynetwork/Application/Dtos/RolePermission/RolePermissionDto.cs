namespace thirdpartynetwork.Application.Dtos.RolePermission
{
    public class RolePermissionDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public RoleLightDto Role { get; set; }
        public required PermissionLightDto Permission { get; set; }
    }
    
    public class RolePermissionRtPDto
    {
        public required PermissionLightDto Permission { get; set; }
    }
    
    public class RolePermissionPtRDto
    {
        public RoleLightDto Role { get; set; }
    }
}