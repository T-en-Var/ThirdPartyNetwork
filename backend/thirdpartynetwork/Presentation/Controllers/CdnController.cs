using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Enum;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Presentation.Controllers;

/// <summary>
/// PLAN.md §5/§2.3: deliberately unauthenticated — no [RequirePermission], no bearer token
/// expected, the token itself is the credential. nginx routes files.&lt;domain&gt;/{token} here
/// (as api.&lt;domain&gt;/cdn/{token}); this is the ONLY route a ContentViewingLink's ContentKind
/// can be File for — the other four kinds resolve through GET /view/{token} instead.
/// </summary>
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

        // A NEW MinIO signature every time (§2.3's "don't use the same link" note) — the
        // ContentViewingLink token and this presigned URL are two different, both
        // single-use-ish, layers.
        var expiry = result.Link.Mode == ContentViewingLinkMode.TimeLimited
            ? cdnService.StreamingPresignExpiry
            : cdnService.DefaultPresignExpiry;

        var presignedUrl = await cdnService.GetPresignedDownloadUrlAsync(file.Bucket, file.ObjectKey, expiry);
        return Redirect(presignedUrl.ToString());
    }
}
