namespace trojoaccounthub.Models
{
    public class NetworkRole
    {
        public int NetworkId { get; set; }
        public int RoleId { get; set; }
        public required Network Network { get; set; }
        public required Role Role { get; set; }
    }
}
