namespace trojoaccounthub.Dtos.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public ICollection<UserProxyDto> UserProxies { get; set; } = [];
    }
}
