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
        public required User User { get; set; }
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
        
        public UserProxy ApplyDefaults()
        {
            if (IsDefault) return this;
            
            var defaultProxy = this.User.DefaultProxy;

            return new UserProxy
            {
                Id = this.Id,
                Username = this.Username ?? defaultProxy.Username,
                FirstName = this.FirstName ?? defaultProxy.FirstName,
                LastName = this.LastName ?? defaultProxy.LastName,
                Email = this.Email ?? defaultProxy.Email,
                ImageFile = this.ImageFile ?? defaultProxy.ImageFile,
                ImageFileId = this.ImageFileId ?? defaultProxy.ImageFileId,
                IsDefault = this.IsDefault,
                User = this.User,
                NetworkUsers = this.NetworkUsers,
            };
        }
    }
}
