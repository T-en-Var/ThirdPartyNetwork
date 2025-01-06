namespace thirdpartynetwork.Dtos.UserProxy
{
    public class CreateUserProxyDto
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ProfilePicture { get; set; }
        public required bool IsDefault { get; set; }
    }
}
