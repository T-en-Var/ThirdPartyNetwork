using System.Text.Json.Serialization;

namespace thirdpartynetwork.Core.Models
{
    public class UserProxy : BaseModel
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public File? ImageFile { get; set; }
        public Guid? ImageFileId { get; set; }
        public required bool IsDefault { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        
        public void ApplyDefaults()
        {
            if (IsDefault) return;
            
            var defaultProxy = User.DefaultProxy;

             Username ??= defaultProxy.Username;
             FirstName ??= defaultProxy.FirstName;
             LastName ??= defaultProxy.LastName;
             Email ??= defaultProxy.Email;
             ImageFile ??= defaultProxy.ImageFile;
             ImageFileId ??= defaultProxy.ImageFileId;
        }
    }
}
