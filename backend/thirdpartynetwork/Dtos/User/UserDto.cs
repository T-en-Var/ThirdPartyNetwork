using thirdpartynetwork.Dtos.UserProxy;

namespace thirdpartynetwork.Dtos.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public ICollection<UserProxyDto> UserProxies { get; set; } = [];
    }
}
