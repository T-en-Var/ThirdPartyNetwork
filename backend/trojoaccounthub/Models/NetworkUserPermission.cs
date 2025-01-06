using System.Security.Cryptography.X509Certificates;

namespace trojoaccounthub.Models
{
    public class NetworkUserPermission
    {
        public Guid NetworkUserId { get; set; }
        public Guid PermissionId { get; set; }
        public required bool IsAccepted { get; set; }
        public required NetworkUser NetworkUser { get; set; }
        public required Permission Permission { get; set; }
    }
}
