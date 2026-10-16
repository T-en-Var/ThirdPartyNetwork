using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

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
            ContentKind.CustomPage => Ok(mapper.Map<CustomPageDto>((CustomPage)result.Content, opts => opts.Items["networkId"] = networkId)),
            _ => NotFound(),
        };
    }
}
