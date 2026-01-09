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
    public async Task<IActionResult> GetNetworkBlogs(Guid networkId)
    {
        var list = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<BlogDto>>(list));
    }

    [HttpGet("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBlog(Guid networkId, Guid blogId)
    {
        var blog = await service.GetByIdAsync(networkId, blogId);
        return blog != null ? Ok(mapper.Map<BlogDto>(blog)) : NotFound();
    }

    [HttpGet("{slug}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySlug(Guid networkId, string slug)
    {
        var blog = await service.GetBySlugAsync(networkId, slug);
        return blog != null ? Ok(mapper.Map<BlogDto>(blog)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(201, Type = typeof(BlogDto))]
    public async Task<IActionResult> CreateBlog(Guid networkId, [FromBody] CreateBlogDto blog)
    {
        var created = await service.CreateAsync(networkId, blog);
        return CreatedAtAction(nameof(CreateBlog), new { networkId = networkId, blogId = created.Id },
            mapper.Map<BlogDto>(created));
    }

    [HttpPut("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBlog(Guid networkId, Guid blogId, [FromBody] UpdateBlogDto updateBlog)
    {
        var updated = await service.UpdateAsync(networkId, blogId, updateBlog);
        return Ok(mapper.Map<BlogDto>(updated));
    }

    [HttpDelete("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteBlog(Guid networkId, Guid blogId)
    {
        await service.DeleteByIdAsync(networkId, blogId);
        return NoContent();
    }
}