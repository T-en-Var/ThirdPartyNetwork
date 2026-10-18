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

        public bool IsSystemProtected { get; set; } = false;

        // One-to-one relationship
        public Guid? ImageFileId { get; set; }
        public File? ImageFile { get; set; }
        public virtual required NetworkFeatureFlag FeatureFlag { get; set; }

        // One-to-many
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<NetworkAccess> NetworkAccesses { get; set; } = new List<NetworkAccess>();
        
        // UserContent
        public ICollection<File> Files { get; } = new List<File>();
        public ICollection<Blog> Blogs { get; } = new List<Blog>();
        public ICollection<Configuration> Configurations { get; } = new List<Configuration>();
        public ICollection<CustomPage> CustomPages { get; } = new List<CustomPage>();
        public ICollection<PageBlock> PageBlocks { get; } = new List<PageBlock>();

        public ContentKindFlags ExternalContentKinds { get; set; } = ContentKindFlags.File;
    }
}