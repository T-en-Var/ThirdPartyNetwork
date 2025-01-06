namespace trojoaccounthub.Models
{
    public class Content
    {
        public Guid Id { get; set; }
        public required string Value { get; set; }
        public required Network Network { get; set; }
    }
}
