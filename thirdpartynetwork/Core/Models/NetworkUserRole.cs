namespace thirdpartynetwork.Core.Models
{
    public class NetworkUserRole : BaseModel
    {
        public Guid NetworkUserId { get; set; }
        public Guid RoleId { get; set; }
        public required NetworkUser NetworkUser { get; set; }
        public required Role Role { get; set; }
    }
}
