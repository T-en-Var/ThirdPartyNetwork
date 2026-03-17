using System.Text.Json.Serialization;

namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class UserProxyDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public FileDto? ImageFile { get; set; }
        public required bool IsDefault { get; set; }
        public required bool HasPassword { get; set; }
        public DateTime CreatedOn { get; set; }
        public required UserLightDto User { get; set; }
        public ICollection<NetworkUserLightDto> NetworkUsers { get; set; } = [];
    }
    
    public class UserProxyLightDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public FileDto? ImageFile { get; set; }
        public required bool IsDefault { get; set; }
        public required bool HasPassword { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    
    public class UserProxyLightShortProfileDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public FileDto? ImageFile { get; set; }
        public required bool IsDefault { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    
    public class UserProxyLightNoCredentialsDto
    {
        public Guid Id { get; set; }
        public required bool IsDefault { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    
    public class UserProxyLightWithNetworksDto : UserProxyLightDto
    {
        public ICollection<NetworkUserLightWithNetworkDto> NetworkUsers { get; set; } = [];
    }
}
