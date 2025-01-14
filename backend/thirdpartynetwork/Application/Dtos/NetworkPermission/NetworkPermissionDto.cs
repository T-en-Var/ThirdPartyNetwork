namespace thirdpartynetwork.Application.Dtos.NetworkPermission
{
    public class NetworkPermissionDto
    {
        public Guid NetworkId { get; set; }
        public Guid PermissionId { get; set; }
        public required bool IsRequired { get; set; }
        public required PermissionDto Permission { get; set; }
        public required NetworkDto Network { get; set; }
    }
}
