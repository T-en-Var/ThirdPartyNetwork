using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/customPages/")]
[ApiController]
public class CustomPageController(
    ICustomPageService service,
    IContentAccessGrantService accessGrantService,
    IContentViewingLinkService viewingLinkService,
    IContentModerationFlagService moderationFlagService,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadCustomPage, IsListEndpoint = true)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<PageDto>))]
    public async Task<IActionResult> GetNetworkCustomPages(Guid networkId)
    {
        var customPage = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<PageDto>>(customPage, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ReadCustomPage)]
    [ProducesResponseType(200, Type = typeof(PageDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCustomPage(Guid networkId, Guid customPageId)
    {
        var customPage = await service.GetByIdAsync(networkId, customPageId);
        if (customPage == null) return NotFound();

        var dto = mapper.Map<PageDto>(customPage, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(customPage, ContentKind.CustomPage, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(201, Type = typeof(PageDto))]
    public async Task<IActionResult> CreateCustomPage(Guid networkId, [FromBody] CreateCustomPageDto customPage)
    {
        var uploadedCustomPage = await service.CreateAsync(networkId, customPage);
        return Ok(mapper.Map<PageDto>(uploadedCustomPage, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPut("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(200, Type = typeof(PageDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCustomPage(Guid networkId, Guid customPageId,
        [FromBody] UpdateCustomPageDto updateCustomPage)
    {
        var customPage = await service.UpdateAsync(networkId, customPageId, updateCustomPage);
        return Ok(mapper.Map<PageDto>(customPage, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpDelete("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteCustomPage(Guid networkId, Guid customPageId)
    {
        await service.DeleteByIdAsync(networkId, customPageId);
        return NoContent();
    }

    [HttpPost("{customPageId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishCustomPage(Guid networkId, Guid customPageId)
    {
        var customPage = await service.GetByIdAsync(networkId, customPageId);
        if (customPage == null) return NotFound();
        var published = await service.PublishAsync(customPage);
        return Ok(mapper.Map<PageDto>(published, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{customPageId:guid}/unpublish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishCustomPage(Guid networkId, Guid customPageId)
    {
        var customPage = await service.GetByIdAsync(networkId, customPageId);
        if (customPage == null) return NotFound();
        var unpublished = await service.UnpublishAsync(customPage);
        return Ok(mapper.Map<PageDto>(unpublished, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{customPageId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(PageDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetCustomPageOwnershipMode(Guid networkId, Guid customPageId, [FromBody] SetOwnershipModeDto dto)
    {
        var customPage = await service.GetByIdAsync(networkId, customPageId);
        if (customPage == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(customPage, dto.Mode);
        return Ok(mapper.Map<PageDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{customPageId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentAccessGrantDto>))]
    public async Task<IActionResult> GetCustomPageAccessGrants(Guid networkId, Guid customPageId)
    {
        var grants = await accessGrantService.GetForContentAsync(ContentKind.CustomPage, customPageId);
        return Ok(mapper.Map<IEnumerable<ContentAccessGrantDto>>(grants));
    }

    [HttpPost("{customPageId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(201, Type = typeof(ContentAccessGrantDto))]
    public async Task<IActionResult> CreateCustomPageAccessGrant(Guid networkId, Guid customPageId, [FromBody] CreateContentAccessGrantDto dto)
    {
        var grant = await accessGrantService.CreateAsync(networkId, ContentKind.CustomPage, customPageId, dto);
        return Ok(mapper.Map<ContentAccessGrantDto>(grant));
    }

    [HttpDelete("{customPageId:guid}/access/{grantId:guid}")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RevokeCustomPageAccessGrant(Guid networkId, Guid customPageId, Guid grantId)
    {
        await accessGrantService.RevokeAsync(networkId, grantId);
        return NoContent();
    }

    [HttpGet("{customPageId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateCustomPage)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentModerationFlagDto>))]
    public async Task<IActionResult> GetCustomPageModerationFlags(Guid networkId, Guid customPageId)
    {
        var flags = await moderationFlagService.GetForContentAsync(ContentKind.CustomPage, customPageId);
        return Ok(mapper.Map<IEnumerable<ContentModerationFlagDto>>(flags));
    }

    [HttpPost("{customPageId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateCustomPage)]
    [ProducesResponseType(201, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> CreateCustomPageModerationFlag(Guid networkId, Guid customPageId, [FromBody] CreateContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.CreateAsync(networkId, ContentKind.CustomPage, customPageId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }

    [HttpPut("{customPageId:guid}/moderation-flags/{flagId:guid}/resolve")]
    [RequirePermission(Constants.Permissions.ModerateCustomPage)]
    [ProducesResponseType(200, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> ResolveCustomPageModerationFlag(Guid networkId, Guid customPageId, Guid flagId, [FromBody] ResolveContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.ResolveAsync(networkId, flagId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }
}