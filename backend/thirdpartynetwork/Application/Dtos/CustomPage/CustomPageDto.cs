namespace thirdpartynetwork.Application.Dtos.CustomPage;

public class CustomPageDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public Guid NetworkId { get; set; }
    public NetworkUserDto Author { get; set; }
    public ICollection<PageBlockDto> Pages { get; set; }
}