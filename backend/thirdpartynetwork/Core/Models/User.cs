namespace thirdpartynetwork.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public ICollection<UserProxy> Proxies { get; set; } = new List<UserProxy>();
    }
}
