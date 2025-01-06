namespace trojoaccounthub.Dtos.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public ICollection<UserProxyDto> UserProxies { get; set; } = [];
    }
}
