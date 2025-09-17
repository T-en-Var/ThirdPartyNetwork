using System.ComponentModel.DataAnnotations;

namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class AddUserProxyDto
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public FileLinkDto? ImageFile { get; set; }
    }
}
