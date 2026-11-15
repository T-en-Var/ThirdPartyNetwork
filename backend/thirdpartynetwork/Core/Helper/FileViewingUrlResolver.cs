using AutoMapper;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Helper;

public class FileViewingUrlResolver(ICDNService cdnService) : IValueResolver<File, FileDto, string?>
{
    public string? Resolve(File source, FileDto destination, string? destMember, ResolutionContext context)
    {
        return source.Bucket == cdnService.PublicBucketName
            ? cdnService.GetPublicUrl(source.Bucket, source.ObjectKey).ToString()
            : null;
    }
}
