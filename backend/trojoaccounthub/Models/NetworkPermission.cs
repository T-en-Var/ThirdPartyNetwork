namespace trojoaccounthub.Models
{
    public class NetworkPermission
    {
        public int NetworkId { get; set; }
        public int PermissionId { get; set; }
        public required bool IsRequired { get; set; }
        public required Network Network { get; set; }
        public required Permission Permission { get; set; }
    }
}
