using System.Text.Json;

namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class NetworkUserDto
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public required UserProxyLightDto UserProxy { get; set; }
        public required NetworkLightDto Network { get; set; }
        public bool AccessIncomplete { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<NetworkUserAccessNtADto> NetworkUserAccesses { get; set; } = [];
        public ICollection<NetworkUserRoleNtRDto> NetworkUserRoles { get; set; } = [];
        public required JsonElement Entitlements { get; set; }
    }
    
    public class NetworkUserLightDto
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public required UserProxyLightShortProfileDto UserProxy { get; set; }
        public bool AccessIncomplete { get; set; }
        public DateTime CreatedOn { get; set; }
        public required JsonElement Entitlements { get; set; }
    }
    
    public class NetworkUserLightUserInfoDto
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public bool AccessIncomplete { get; set; }
        public DateTime CreatedOn { get; set; }
        public required JsonElement Entitlements { get; set; }
        public required UserProxyLightDto UserProxy { get; set; }
        public ICollection<NetworkUserAccessNtADto> NetworkUserAccesses { get; set; } = [];
        public ICollection<NetworkUserRoleNtRDto> NetworkUserRoles { get; set; } = [];
    }
    
    public class NetworkUserLightWithNetworkDto
    {
        public Guid Id { get; set; }
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
        public bool AccessIncomplete { get; set; }
        public DateTime CreatedOn { get; set; }
        public required JsonElement Entitlements { get; set; }
        public required NetworkLightDto Network { get; set; }
    }
}
