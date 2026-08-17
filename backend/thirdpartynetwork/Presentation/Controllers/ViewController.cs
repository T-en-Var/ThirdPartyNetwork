using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;
using thirdpartynetwork.Application.Dtos.UserContent.CustomPage;
using thirdpartynetwork.Application.Dtos.UserContent.PageBlock;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

/// <summary>
/// PLAN.md §2.3/§5/§6: the generic front door for the four content kinds that aren't File —
/// there's no object-storage payload to redirect to for a Blog/Configuration/CustomPage/
/// PageBlock, so the token just re-authorizes a normal API read instead of a byte transfer.
/// Deliberately unauthenticated, same as GET /cdn/{token} — the token is the credential.
/// </summary>
[Route("view")]
[ApiController]
public class ViewController(IContentViewingLinkService linkService, IMapper mapper) : ControllerBase
{
    [HttpGet("{token:guid}")]
    public async Task<IActionResult> Resolve(Guid token)
    {
        var result = await linkService.ResolveAsync(token, expectedKind: null);
        if (result == null) return NotFound();

        var networkId = result.Content.NetworkId;

        return result.Link.ContentKind switch
        {
            ContentKind.Blog => Ok(mapper.Map<BlogDto>((Blog)result.Content, opts => opts.Items["networkId"] = networkId)),
            ContentKind.Configuration => Ok(mapper.Map<ConfigurationDto>((Configuration)result.Content, opts => opts.Items["networkId"] = networkId)),
            ContentKind.CustomPage => Ok(mapper.Map<PageDto>((CustomPage)result.Content, opts => opts.Items["networkId"] = networkId)),
            ContentKind.PageBlock => Ok(mapper.Map<PageBlockDto>((PageBlock)result.Content, opts => opts.Items["networkId"] = networkId)),
            _ => NotFound(), // File never reaches here — ResolveAsync rejects it (§2.3)
        };
    }
}
