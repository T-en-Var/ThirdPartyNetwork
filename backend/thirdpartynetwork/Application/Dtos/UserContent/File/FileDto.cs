using thirdpartynetwork.Application.Dtos.UserContent.UserContentBase;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.File
{
    public class FileDto : UserContentBaseDto
    {
        public string Bucket { get; set; }
        public string ObjectKey { get; set; }
        public string? Format { get; set; }
        public long SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public TimeSpan? Duration { get; set; }
        public string MediaType { get; set; }
        public string Name { get; set; }
        public required bool IsSystemProtected { get; set; }
        public string? ViewingUrl { get; set; }
    }

    public class FileLightDto : UserContentBaseLightDto
    {
        public string Bucket { get; set; }
        public string ObjectKey { get; set; }
        public string? Format { get; set; }
        public long SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public TimeSpan? Duration { get; set; }
        public string MediaType { get; set; }
        public string Name { get; set; }
        public required bool IsSystemProtected { get; set; }
    }
}
