using System.Security.Cryptography.X509Certificates;

namespace trojoaccounthub.Models
{
    public class NetworkUserRole
    {
        public int NetworkUserId { get; set; }
        public int RoleId { get; set; }
        public required NetworkUser NetworkUser { get; set; }
        public required Role Role { get; set; }
    }
}
