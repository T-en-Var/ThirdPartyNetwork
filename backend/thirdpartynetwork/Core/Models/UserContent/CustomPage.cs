namespace thirdpartynetwork.Core.Models;

public class CustomPage : UserContentBaseModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public ICollection<PageBlock> Pages { get; set; }
}
