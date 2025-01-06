namespace trojoaccounthub.Models
{
    public class User
    {
        public int Id { get; set; }
        public ICollection<UserProxy> Proxies { get; set; } = new List<UserProxy>();
    }
}
