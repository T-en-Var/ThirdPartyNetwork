namespace trojoaccounthub.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<NetworkRole> NetworkRoles { get; set; } = new List<NetworkRole>();
        public ICollection<NetworkUserRole> NetworkUserRoles { get; set; } = new List<NetworkUserRole>();
    }
}
