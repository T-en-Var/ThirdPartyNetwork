namespace trojoaccounthub.Models
{
    public class NetworkPermission
    {
        public Guid NetworkId { get; set; }
        public Guid PermissionId { get; set; }
        public required bool IsRequired { get; set; }
        public required Network Network { get; set; }
        public required Permission Permission { get; set; }
    }
}
