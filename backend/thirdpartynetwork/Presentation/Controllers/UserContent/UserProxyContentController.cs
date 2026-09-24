using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("users/{userId:guid}/proxies/{userProxyId:guid}")]
[ApiController]
public class UserProxyContentController(
    IBlogService blogService,
    IConfigurationService configurationService,
    ICustomPageService customPageService,
    IFileService fileService,
    IMapper mapper) : ControllerBase
{
    [HttpGet("blogs")]
    [RequirePermission(Constants.Permissions.IsUserOwner, Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BlogDto>))]
    public async Task<IActionResult> GetBlogs(Guid userProxyId)
    {
        var blogs = await blogService.GetByUserProxyIdAsync(userProxyId);
        return Ok(mapper.Map<IEnumerable<BlogDto>>(blogs, opts => { opts.Items["bypass"] = true; }));
    }
    
    [HttpGet("configurations")]
    [RequirePermission(Constants.Permissions.IsUserOwner, Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ConfigurationDto>))]
    public async Task<IActionResult> GetConfigurations(Guid userProxyId)
    {
        var configurations = await configurationService.GetByUserProxyIdAsync(userProxyId);
        return Ok(mapper.Map<IEnumerable<ConfigurationDto>>(configurations, opts => { opts.Items["bypass"] = true; }));
    }
    
    [HttpGet("customPages")]
    [RequirePermission(Constants.Permissions.IsUserOwner, Constants.Permissions.ReadCustomPage)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CustomPageDto>))]
    public async Task<IActionResult> GetCustomPages(Guid userProxyId)
    {
        var customPages = await customPageService.GetByUserProxyIdAsync(userProxyId);
        return Ok(mapper.Map<IEnumerable<CustomPageDto>>(customPages, opts => { opts.Items["bypass"] = true; }));
    }
    
    [HttpGet("files")]
    [RequirePermission(Constants.Permissions.IsUserOwner, Constants.Permissions.ReadFile)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
    public async Task<IActionResult> GetFiles(Guid userProxyId)
    {
        var files = await fileService.GetByUserProxyIdAsync(userProxyId);
        return Ok(mapper.Map<IEnumerable<FileDto>>(files, opts => { opts.Items["bypass"] = true; }));
    }
}