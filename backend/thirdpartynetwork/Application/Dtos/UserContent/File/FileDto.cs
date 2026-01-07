using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.File
{
    public class FileDto : CustomUserContentBaseDto
    {
        public Guid Id { get; set; }
        public Guid PublicId { get; set; }
        public string Url { get; set; }
        public string? Format { get; set; }
        public long SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public TimeSpan? Duration { get; set; }
        public string MediaType { get; set; } // e.g., image, video
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required bool IsSystemProtected { get; set; }
    }
}
