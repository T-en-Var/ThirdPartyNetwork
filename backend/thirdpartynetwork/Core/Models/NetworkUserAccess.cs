namespace thirdpartynetwork.Core.Models
{
    public class NetworkUserAccess : BaseModel
    {
        public Guid NetworkUserId { get; set; }
        public Guid AccessId { get; set; }
        public required bool IsAccepted { get; set; }
        public required NetworkUser NetworkUser { get; set; }
        public required Access Access { get; set; }
    }
}
