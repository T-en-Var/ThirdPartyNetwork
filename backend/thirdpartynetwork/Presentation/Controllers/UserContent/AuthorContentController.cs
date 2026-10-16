using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;
using thirdpartynetwork.Application.Dtos.UserContent.CustomPage;
using thirdpartynetwork.Application.Dtos.UserContent.File;

namespace thirdpartynetwork.Presentation.Controllers;

// No [Authorize]/[RequirePermission] here on purpose: this is a public, anonymous-reachable
// endpoint. Visibility is enforced per item by AccessLevelVerifier.GetCrossNetworkAccessPredicateAsync
// inside UserContentBaseRepository.GetByUserProxyIdAsync, not by a controller-level gate.
[Route("authors/{userProxyId:guid}")]
[ApiController]
public class AuthorContentController(
    IBlogService blogService,
    IConfigurationService configurationService,
    ICustomPageService customPageService,
    IFileService fileService,
    IMapper mapper) : ControllerBase
{
    [HttpGet("blogs")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BlogDto>))]
    public async Task<IActionResult> GetBlogs(Guid userProxyId)
    {
        var blogs = await blogService.GetByUserProxyIdAsync(userProxyId);
        return Ok(blogs.Select(MapWithNetworkContext<BlogDto>));
    }

    [HttpGet("configurations")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ConfigurationDto>))]
    public async Task<IActionResult> GetConfigurations(Guid userProxyId)
    {
        var configurations = await configurationService.GetByUserProxyIdAsync(userProxyId);
        return Ok(configurations.Select(MapWithNetworkContext<ConfigurationDto>));
    }

    [HttpGet("customPages")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CustomPageDto>))]
    public async Task<IActionResult> GetCustomPages(Guid userProxyId)
    {
        var customPages = await customPageService.GetByUserProxyIdAsync(userProxyId);
        return Ok(customPages.Select(MapWithNetworkContext<CustomPageDto>));
    }

    [HttpGet("files")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
    public async Task<IActionResult> GetFiles(Guid userProxyId)
    {
        var files = await fileService.GetByUserProxyIdAsync(userProxyId);
        return Ok(files.Select(MapWithNetworkContext<FileDto>));
    }

    // Content spans whichever networks this author belongs to, so each item is mapped with its
    // OWN NetworkId as context. That keeps the per-network profile-visibility consent check
    // (MapFieldIfAccessGranted) scoped correctly instead of bypassing it for the whole list.
    private TDto MapWithNetworkContext<TDto>(Core.Models.UserContentBaseModel source)
    {
        return mapper.Map<TDto>(source, opts => opts.Items["networkId"] = source.NetworkId);
    }
}
