using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.UserContent.File
{
    public class UpdateFileDto
    {
        public required string Name { get; set; }
        public required AccessLevel AccessLevel { get; set; }
    }
}