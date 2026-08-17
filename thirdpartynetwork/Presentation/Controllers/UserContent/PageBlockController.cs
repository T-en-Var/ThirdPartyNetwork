using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/customPages/{customPageId:guid}/pageBlocks")]
[ApiController]
public class PageBlockController(
    IPageBlockService service,
    IContentAccessGrantService accessGrantService,
    IContentViewingLinkService viewingLinkService,
    IContentModerationFlagService moderationFlagService,
    IMapper mapper) : ControllerBase
{
    [HttpGet("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ReadPageBlock)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        if (pageBlock == null) return NotFound();

        var dto = mapper.Map<PageBlockDto>(pageBlock, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(pageBlock, ContentKind.PageBlock, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(201, Type = typeof(PageBlockDto))]
    public async Task<IActionResult> CreatePageBlock(Guid networkId, Guid customPageId, [FromBody] CreatePageBlockDto pageBlock)
    {
        var uploadedPageBlock = await service.CreateAsync(networkId, customPageId, pageBlock);
        return Ok(mapper.Map<PageBlockDto>(uploadedPageBlock, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPut("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePageBlock(Guid networkId, Guid customPageId, Guid pageBlockId,
        [FromBody] UpdatePageBlockDto updatePageBlock)
    {
        var pageBlock = await service.UpdateAsync(networkId, customPageId, pageBlockId, updatePageBlock);
        return Ok(mapper.Map<PageBlockDto>(pageBlock, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpDelete("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeletePageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        await service.DeleteByIdAsync(networkId, customPageId, pageBlockId);
        return NoContent();
    }

    // NOTE (PLAN.md §2.3 open question): PageBlock's own visibility is already vestigial —
    // delegated to its parent CustomPage (DATABASE_AND_AUTH_OVERVIEW.md §5.3). These
    // endpoints exist for symmetry with the other four content types, but whether a
    // standalone PageBlock-level publish/grant is product-meaningful (vs. always managing
    // sharing at the CustomPage level) is an open call, not decided here.
    [HttpPost("{pageBlockId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishPageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        if (pageBlock == null) return NotFound();
        var published = await service.PublishAsync(pageBlock);
        return Ok(mapper.Map<PageBlockDto>(published, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{pageBlockId:guid}/unpublish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishPageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        if (pageBlock == null) return NotFound();
        var unpublished = await service.UnpublishAsync(pageBlock);
        return Ok(mapper.Map<PageBlockDto>(unpublished, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{pageBlockId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetPageBlockOwnershipMode(Guid networkId, Guid customPageId, Guid pageBlockId, [FromBody] SetOwnershipModeDto dto)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        if (pageBlock == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(pageBlock, dto.Mode);
        return Ok(mapper.Map<PageBlockDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{pageBlockId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentAccessGrantDto>))]
    public async Task<IActionResult> GetPageBlockAccessGrants(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var grants = await accessGrantService.GetForContentAsync(ContentKind.PageBlock, pageBlockId);
        return Ok(mapper.Map<IEnumerable<ContentAccessGrantDto>>(grants));
    }

    [HttpPost("{pageBlockId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(201, Type = typeof(ContentAccessGrantDto))]
    public async Task<IActionResult> CreatePageBlockAccessGrant(Guid networkId, Guid customPageId, Guid pageBlockId, [FromBody] CreateContentAccessGrantDto dto)
    {
        var grant = await accessGrantService.CreateAsync(networkId, ContentKind.PageBlock, pageBlockId, dto);
        return Ok(mapper.Map<ContentAccessGrantDto>(grant));
    }

    [HttpDelete("{pageBlockId:guid}/access/{grantId:guid}")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RevokePageBlockAccessGrant(Guid networkId, Guid customPageId, Guid pageBlockId, Guid grantId)
    {
        await accessGrantService.RevokeAsync(networkId, grantId);
        return NoContent();
    }

    [HttpGet("{pageBlockId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModeratePageBlock)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentModerationFlagDto>))]
    public async Task<IActionResult> GetPageBlockModerationFlags(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var flags = await moderationFlagService.GetForContentAsync(ContentKind.PageBlock, pageBlockId);
        return Ok(mapper.Map<IEnumerable<ContentModerationFlagDto>>(flags));
    }

    [HttpPost("{pageBlockId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModeratePageBlock)]
    [ProducesResponseType(201, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> CreatePageBlockModerationFlag(Guid networkId, Guid customPageId, Guid pageBlockId, [FromBody] CreateContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.CreateAsync(networkId, ContentKind.PageBlock, pageBlockId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }

    [HttpPut("{pageBlockId:guid}/moderation-flags/{flagId:guid}/resolve")]
    [RequirePermission(Constants.Permissions.ModeratePageBlock)]
    [ProducesResponseType(200, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> ResolvePageBlockModerationFlag(Guid networkId, Guid customPageId, Guid pageBlockId, Guid flagId, [FromBody] ResolveContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.ResolveAsync(networkId, flagId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }
}