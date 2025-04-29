namespace thirdpartynetwork.Application.Dtos.Content
{
    public class ContentDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<NetworkDto> Networks { get; set; } = [];
    }
}
