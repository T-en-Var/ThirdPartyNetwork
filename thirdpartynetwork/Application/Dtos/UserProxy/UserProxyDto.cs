using thirdpartynetwork.Application.Dtos.User;

namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class UserProxyDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? ImageUrl { get; set; }
        public required bool IsDefault { get; set; }
        public DateTime CreatedOn { get; set; }
        public required UserDto User { get; set; }
    }
}
