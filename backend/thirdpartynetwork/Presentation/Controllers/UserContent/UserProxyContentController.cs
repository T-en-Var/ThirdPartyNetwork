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
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BlogDto>))]
    public IActionResult GetBlogs(Guid userProxyId)
    {
        var blogs = blogService.GetByUserProxyId(userProxyId);
        return Ok(mapper.Map<IEnumerable<BlogDto>>(blogs));
    }
    
    [HttpGet("configurations")]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ConfigurationDto>))]
    public IActionResult GetConfigurations(Guid userProxyId)
    {
        var configurations = configurationService.GetByUserProxyId(userProxyId);
        return Ok(mapper.Map<IEnumerable<ConfigurationDto>>(configurations));
    }
    
    [HttpGet("customPages")]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CustomPageDto>))]
    public IActionResult GetCustomPages(Guid userProxyId)
    {
        var customPages = customPageService.GetByUserProxyId(userProxyId);
        return Ok(mapper.Map<IEnumerable<CustomPageDto>>(customPages));
    }
    
    [HttpGet("files")]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
    public IActionResult GetFiles(Guid userProxyId)
    {
        var files = fileService.GetByUserProxyId(userProxyId);
        return Ok(mapper.Map<IEnumerable<FileDto>>(files));
    }
}