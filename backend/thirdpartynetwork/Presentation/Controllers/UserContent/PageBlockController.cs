using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/customPages/{customPageId:guid}/pageBlocks")]
[ApiController]
public class PageBlockController(IPageBlockService service, IMapper mapper) : ControllerBase
{
    [HttpGet("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ReadPageBlock)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        var pageBlock = await service.GetByIdAsync(networkId, pageBlockId);
        return pageBlock != null ? Ok(mapper.Map<PageBlockDto>(pageBlock)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(201, Type = typeof(PageBlockDto))]
    public async Task<IActionResult> CreatePageBlock(Guid networkId, Guid customPageId, [FromBody] CreatePageBlockDto pageBlock)
    {
        var uploadedPageBlock = await service.CreateAsync(networkId, customPageId, pageBlock);
        return Ok(mapper.Map<PageBlockDto>(uploadedPageBlock));
    }

    [HttpPut("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(200, Type = typeof(PageBlockDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePageBlock(Guid networkId, Guid customPageId, Guid pageBlockId,
        [FromBody] UpdatePageBlockDto updatePageBlock)
    {
        var pageBlock = await service.UpdateAsync(networkId, customPageId, pageBlockId, updatePageBlock);
        return Ok(mapper.Map<PageBlockDto>(pageBlock));
    }

    [HttpDelete("{pageBlockId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePageBlock)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeletePageBlock(Guid networkId, Guid customPageId, Guid pageBlockId)
    {
        await service.DeleteByIdAsync(networkId, customPageId, pageBlockId);
        return NoContent();
    }
}