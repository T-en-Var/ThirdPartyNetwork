using System.ComponentModel.DataAnnotations;

namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class CreateUserProxyDto
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string ProfilePicture { get; set; } = "DefaultImage";
        // public required bool IsDefault { get; set; }
    }
}
