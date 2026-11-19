namespace thirdpartynetwork.Application.Dtos.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<UserProxyLightDto> UserProxies { get; set; } = [];
    }
    
    public class UserWithNetworksDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<UserProxyLightWithNetworksDto> UserProxies { get; set; } = [];
    }
    
    public class UserLightDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
