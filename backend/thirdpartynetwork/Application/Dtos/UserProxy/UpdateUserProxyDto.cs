namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class UpdateUserProxyDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        // public string? ImageUrl { get; set; }
        public required bool KeepPassword { get; set; }
        public required bool IsDefault { get; set; }
        public FileLinkDto? FileLink { get; set; }
    }
}
