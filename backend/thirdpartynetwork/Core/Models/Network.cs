using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Core.Models
{
    public class Network : BaseModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required bool IsPublic { get; set; }
        public required string Description { get; set; }
        public required string RedirectURI { get; set; }

        // PLAN.md §2.6: which of this network's Public content kinds another network
        // (never specifically invited via a ContentAccessGrant) may durably reuse/embed.
        // Default File-only — the doc's own recurring example (shared branding/profile
        // images) as the one kind reusable out of the box.
        public ContentKindFlags ExternalContentKinds { get; set; } = ContentKindFlags.File;

        // One-to-one relationship
        public Guid? ImageFileId { get; set; }
        public File? ImageFile { get; set; }
        public virtual required NetworkEntitlement Entitlement { get; set; }

        // One-to-many
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        public bool IsSystemProtected { get; set; } = false;
        
        // UserContent
        public ICollection<File> Files { get; } = new List<File>();
        public ICollection<Blog> Blogs { get; } = new List<Blog>();
        public ICollection<Configuration> Configurations { get; } = new List<Configuration>();
        public ICollection<CustomPage> CustomPages { get; } = new List<CustomPage>();
        public ICollection<PageBlock> PageBlocks { get; } = new List<PageBlock>();
    }
}