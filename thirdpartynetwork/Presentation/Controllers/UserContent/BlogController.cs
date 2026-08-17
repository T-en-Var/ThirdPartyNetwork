using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/blogs")]
[ApiController]
public class BlogController(
    IBlogService service,
    IContentAccessGrantService accessGrantService,
    IContentViewingLinkService viewingLinkService,
    IContentModerationFlagService moderationFlagService,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BlogDto>))]
    public async Task<IActionResult> GetNetworkBlogs(Guid networkId)
    {
        var list = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<BlogDto>>(list, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBlog(Guid networkId, Guid blogId)
    {
        var blog = await service.GetByIdAsync(networkId, blogId);
        if (blog == null) return NotFound();
        return Ok(await MapWithViewingUrlAsync(blog, networkId));
    }

    [HttpGet("{slug}")]
    [RequirePermission(Constants.Permissions.ReadBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySlug(Guid networkId, string slug)
    {
        var blog = await service.GetBySlugAsync(networkId, slug);
        if (blog == null) return NotFound();
        return Ok(await MapWithViewingUrlAsync(blog, networkId));
    }

    private async Task<BlogDto> MapWithViewingUrlAsync(Blog blog, Guid networkId)
    {
        var dto = mapper.Map<BlogDto>(blog, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(blog, ContentKind.Blog, Request.Scheme, Request.Host.ToString());
        return dto;
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(201, Type = typeof(BlogDto))]
    public async Task<IActionResult> CreateBlog(Guid networkId, [FromBody] CreateBlogDto blog)
    {
        var created = await service.CreateAsync(networkId, blog);
        return Ok(mapper.Map<BlogDto>(created, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPut("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBlog(Guid networkId, Guid blogId, [FromBody] UpdateBlogDto updateBlog)
    {
        var updated = await service.UpdateAsync(networkId, blogId, updateBlog);
        return Ok(mapper.Map<BlogDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpDelete("{blogId:guid}")]
    [RequirePermission(Constants.Permissions.ManageBlog)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteBlog(Guid networkId, Guid blogId)
    {
        await service.DeleteByIdAsync(networkId, blogId);
        return NoContent();
    }

    [HttpPost("{blogId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishBlog(Guid networkId, Guid blogId)
    {
        var blog = await service.GetByIdAsync(networkId, blogId);
        if (blog == null) return NotFound();
        var published = await service.PublishAsync(blog);
        return Ok(mapper.Map<BlogDto>(published, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{blogId:guid}/unpublish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishBlog(Guid networkId, Guid blogId)
    {
        var blog = await service.GetByIdAsync(networkId, blogId);
        if (blog == null) return NotFound();
        var unpublished = await service.UnpublishAsync(blog);
        return Ok(mapper.Map<BlogDto>(unpublished, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{blogId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(BlogDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetBlogOwnershipMode(Guid networkId, Guid blogId, [FromBody] SetOwnershipModeDto dto)
    {
        var blog = await service.GetByIdAsync(networkId, blogId);
        if (blog == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(blog, dto.Mode);
        return Ok(mapper.Map<BlogDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{blogId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentAccessGrantDto>))]
    public async Task<IActionResult> GetBlogAccessGrants(Guid networkId, Guid blogId)
    {
        var grants = await accessGrantService.GetForContentAsync(ContentKind.Blog, blogId);
        return Ok(mapper.Map<IEnumerable<ContentAccessGrantDto>>(grants));
    }

    [HttpPost("{blogId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(201, Type = typeof(ContentAccessGrantDto))]
    public async Task<IActionResult> CreateBlogAccessGrant(Guid networkId, Guid blogId, [FromBody] CreateContentAccessGrantDto dto)
    {
        var grant = await accessGrantService.CreateAsync(networkId, ContentKind.Blog, blogId, dto);
        return Ok(mapper.Map<ContentAccessGrantDto>(grant));
    }

    [HttpDelete("{blogId:guid}/access/{grantId:guid}")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RevokeBlogAccessGrant(Guid networkId, Guid blogId, Guid grantId)
    {
        await accessGrantService.RevokeAsync(networkId, grantId);
        return NoContent();
    }

    [HttpGet("{blogId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateBlog)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentModerationFlagDto>))]
    public async Task<IActionResult> GetBlogModerationFlags(Guid networkId, Guid blogId)
    {
        var flags = await moderationFlagService.GetForContentAsync(ContentKind.Blog, blogId);
        return Ok(mapper.Map<IEnumerable<ContentModerationFlagDto>>(flags));
    }

    [HttpPost("{blogId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateBlog)]
    [ProducesResponseType(201, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> CreateBlogModerationFlag(Guid networkId, Guid blogId, [FromBody] CreateContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.CreateAsync(networkId, ContentKind.Blog, blogId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }

    [HttpPut("{blogId:guid}/moderation-flags/{flagId:guid}/resolve")]
    [RequirePermission(Constants.Permissions.ModerateBlog)]
    [ProducesResponseType(200, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> ResolveBlogModerationFlag(Guid networkId, Guid blogId, Guid flagId, [FromBody] ResolveContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.ResolveAsync(networkId, flagId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }
}