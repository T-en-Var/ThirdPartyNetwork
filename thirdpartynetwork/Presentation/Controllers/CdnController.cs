using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Enum;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("cdn")]
[ApiController]
public class CdnController(IContentViewingLinkService linkService, ICDNService cdnService) : ControllerBase
{
    [HttpGet("{token:guid}")]
    public async Task<IActionResult> Resolve(Guid token)
    {
        var result = await linkService.ResolveAsync(token, ContentKind.File);
        if (result == null) return NotFound();

        var file = (File)result.Content;

        var expiry = result.Link.Mode == ContentViewingLinkMode.TimeLimited
            ? cdnService.StreamingPresignExpiry
            : cdnService.DefaultPresignExpiry;

        var presignedUrl = await cdnService.GetPresignedDownloadUrlAsync(file.Bucket, file.ObjectKey, expiry);
        return Redirect(presignedUrl.ToString());
    }
}
