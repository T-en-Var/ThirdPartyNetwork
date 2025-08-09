using System.Net.Mime;

namespace thirdpartynetwork.Core.Models
{
    public class Network : BaseModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required bool IsPublic { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<Content> Contents { get; set; } = new List<Content>();
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        public ICollection<File> Files { get; set; } = new List<File>();
        public bool IsSystemProtected { get; set; } = false;
    }
}
