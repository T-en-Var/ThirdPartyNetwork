using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Core.Interfaces.Services;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/blogs")]
[ApiController]
public class BlogController(IBlogService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BlogDto>))]
    public IActionResult GetNetworkBlogs(Guid networkId)
    {
        var list = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<BlogDto>>(list));
    }

    [HttpGet("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public IActionResult GetBlog(Guid networkId, Guid blogId)
    {
        var blog = service.GetById(networkId, blogId);
        return blog != null ? Ok(mapper.Map<BlogDto>(blog)) : NotFound();
    }

    [HttpGet("{slug}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public IActionResult GetBySlug(Guid networkId, string slug)
    {
        var blog = service.GetBySlug(networkId, slug);
        return blog != null ? Ok(mapper.Map<BlogDto>(blog)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(201, Type = typeof(BlogDto))]
    public IActionResult CreateBlog(Guid networkId, [FromBody] CreateBlogDto blog)
    {
        var created = service.Create(networkId, blog);
        return CreatedAtAction(nameof(CreateBlog), new { networkId = networkId, blogId = created.Id },
            mapper.Map<BlogDto>(created));
    }

    [HttpPut("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateBlog(Guid networkId, Guid blogId, [FromBody] UpdateBlogDto updateBlog)
    {
        var updated = service.Update(networkId, blogId, updateBlog);
        return Ok(mapper.Map<BlogDto>(updated));
    }

    [HttpDelete("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(204)]
    public IActionResult DeleteBlog(Guid networkId, Guid blogId)
    {
        service.DeleteById(networkId, blogId);
        return NoContent();
    }
}