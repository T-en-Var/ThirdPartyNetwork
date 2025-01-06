namespace trojoaccounthub.Models
{
    public class Content
    {
        public int Id { get; set; }
        public required string Value { get; set; }
        public required Network Network { get; set; }
    }
}
