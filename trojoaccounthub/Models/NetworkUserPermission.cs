using System.Security.Cryptography.X509Certificates;

namespace trojoaccounthub.Models
{
    public class NetworkUserPermission
    {
        public int NetworkUserId { get; set; }
        public int PermissionId { get; set; }
        public required bool IsAccepted { get; set; }
        public required NetworkUser NetworkUser { get; set; }
        public required Permission Permission { get; set; }
    }
}
