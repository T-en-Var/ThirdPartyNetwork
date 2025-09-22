namespace thirdpartynetwork.Core.Models
{
    public class CustomPage : BaseModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public Guid NetworkId { get; set; }
        public Network Network { get; set; }
        public Guid AuthorId { get; set; }
        public NetworkUser Author { get; set; }
        public ICollection<PageBlock> Pages { get; set; }
    }
}