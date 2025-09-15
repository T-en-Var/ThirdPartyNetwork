namespace thirdpartynetwork.Application.Dtos.UserProxy
{
    public class UserProxyDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public FileDto? ImageFile { get; set; }
        public required bool IsDefault { get; set; }
        public DateTime CreatedOn { get; set; }
        public required UserDto User { get; set; }
        public ICollection<NetworkUserDto> NetworkUsers { get; set; }
    }
}
