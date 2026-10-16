using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/customPages/{customPageId:guid}/pageBlocks")]
[ApiController]
public class PageBlockController(
    IPageBlockService service,
    IMapper mapper) : ControllerBase
{
    [HttpGet("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ReadPageBlock, RouteKey = "pageBlockId")]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        if (pageBlock == null) return NotFound();

        var dto = mapper.Map<PageBlockDto>(pageBlock, opts => { opts.Items["networkId"] = networkId; });
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
}