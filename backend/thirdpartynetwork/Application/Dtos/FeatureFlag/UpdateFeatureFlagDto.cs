using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Dtos.FeatureFlag
{
    public class UpdateFeatureFlagDto
    {
        public bool? AllowFiles { get; set; }
        public int? FileCountLimit { get; set; }
        public int? FileSizeLimit { get; set; }
        public int? FileStorageLimit { get; set; }

        public bool? AllowBlogs { get; set; }
        public int? BlogCountLimit { get; set; }

        public bool? AllowConfigurations { get; set; }
        public int? ConfigurationCountLimit { get; set; }

        public bool? AllowCustomPages { get; set; }
        public int? CustomPageCountLimit { get; set; }
        public int? CustomPageBlockCountLimit { get; set; }
        public int? CustomPageBlockSizeLimit { get; set; }  // Character count
    }
}