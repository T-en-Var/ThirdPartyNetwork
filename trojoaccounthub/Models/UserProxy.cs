namespace trojoaccounthub.Models
{
    public class UserProxy
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ProfilePicture { get; set; }
        public required bool IsDefault { get; set; }
        public required User User { get; set; }
        public ICollection<NetworkUser> NetworkUsers { get; set; } = new List<NetworkUser>();
    }
}
