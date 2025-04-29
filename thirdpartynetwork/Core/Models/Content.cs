namespace thirdpartynetwork.Core.Models
{
    public class Content
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Value { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required Guid CreatedBy { get; set; }
        public required Network Network { get; set; }
    }
}
