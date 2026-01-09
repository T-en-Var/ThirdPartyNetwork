using System.ComponentModel.DataAnnotations;

namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class AddUserProxyDto
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public FileLinkDto? FileLink { get; set; }
    }
}
