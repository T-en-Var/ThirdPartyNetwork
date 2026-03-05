namespace thirdpartynetwork.Core.Models
{
    public class Network : BaseModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required bool IsPublic { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }

        // One-to-one relationship
        public Guid? ImageFileId { get; set; }
        public File? ImageFile { get; set; }
        public virtual required NetworkEntitlement Entitlement { get; set; }

        // One-to-many
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        public bool IsSystemProtected { get; set; } = false;
    }
}