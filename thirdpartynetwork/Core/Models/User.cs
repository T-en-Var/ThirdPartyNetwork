namespace thirdpartynetwork.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public ICollection<UserProxy> UserProxies { get; set; } = new List<UserProxy>();
    }
}
