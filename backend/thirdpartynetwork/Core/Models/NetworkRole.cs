namespace thirdpartynetwork.Core.Models
{
    public class NetworkRole
    {
        public Guid NetworkId { get; set; }
        public Guid RoleId { get; set; }
        public required Network Network { get; set; }
        public required Role Role { get; set; }
        public bool IsSystemProtected { get; set; } = false;
    }
}
