using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/configurations")]
[ApiController]
public class ConfigurationController(
    IConfigurationService service,
    IContentAccessGrantService accessGrantService,
    IContentViewingLinkService viewingLinkService,
    IContentModerationFlagService moderationFlagService,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadConfiguration, IsListEndpoint = true)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ConfigurationDto>))]
    public async Task<IActionResult> GetNetworkConfigurations(Guid networkId)
    {
        var list = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<ConfigurationDto>>(list, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ReadConfiguration, RouteKey = "configurationId")]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfiguration(Guid networkId, Guid configurationId)
    {
        var cfg = await service.GetByIdAsync(networkId, configurationId);
        if (cfg == null) return NotFound();

        var dto = mapper.Map<ConfigurationDto>(cfg, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(cfg, ContentKind.Configuration, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(201, Type = typeof(ConfigurationDto))]
    public async Task<IActionResult> CreateConfiguration(Guid networkId, [FromBody] CreateConfigurationDto configuration)
    {
        var created = await service.CreateAsync(networkId, configuration);
        return Ok(mapper.Map<ConfigurationDto>(created, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPut("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateConfiguration(Guid networkId, Guid configurationId,
        [FromBody] UpdateConfigurationDto updateConfiguration)
    {
        var updated = await service.UpdateAsync(networkId, configurationId, updateConfiguration);
        return Ok(mapper.Map<ConfigurationDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpDelete("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteConfiguration(Guid networkId, Guid configurationId)
    {
        await service.DeleteByIdAsync(networkId, configurationId);
        return NoContent();
    }

    [HttpPost("{configurationId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishConfiguration(Guid networkId, Guid configurationId)
    {
        var cfg = await service.GetByIdAsync(networkId, configurationId);
        if (cfg == null) return NotFound();
        var published = await service.PublishAsync(cfg);
        return Ok(mapper.Map<ConfigurationDto>(published, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{configurationId:guid}/unpublish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishConfiguration(Guid networkId, Guid configurationId)
    {
        var cfg = await service.GetByIdAsync(networkId, configurationId);
        if (cfg == null) return NotFound();
        var unpublished = await service.UnpublishAsync(cfg);
        return Ok(mapper.Map<ConfigurationDto>(unpublished, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPost("{configurationId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetConfigurationOwnershipMode(Guid networkId, Guid configurationId, [FromBody] SetOwnershipModeDto dto)
    {
        var cfg = await service.GetByIdAsync(networkId, configurationId);
        if (cfg == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(cfg, dto.Mode);
        return Ok(mapper.Map<ConfigurationDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpGet("{configurationId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentAccessGrantDto>))]
    public async Task<IActionResult> GetConfigurationAccessGrants(Guid networkId, Guid configurationId)
    {
        var grants = await accessGrantService.GetForContentAsync(ContentKind.Configuration, configurationId);
        return Ok(mapper.Map<IEnumerable<ContentAccessGrantDto>>(grants));
    }

    [HttpPost("{configurationId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(201, Type = typeof(ContentAccessGrantDto))]
    public async Task<IActionResult> CreateConfigurationAccessGrant(Guid networkId, Guid configurationId, [FromBody] CreateContentAccessGrantDto dto)
    {
        var grant = await accessGrantService.CreateAsync(networkId, ContentKind.Configuration, configurationId, dto);
        return Ok(mapper.Map<ContentAccessGrantDto>(grant));
    }

    [HttpDelete("{configurationId:guid}/access/{grantId:guid}")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RevokeConfigurationAccessGrant(Guid networkId, Guid configurationId, Guid grantId)
    {
        await accessGrantService.RevokeAsync(networkId, grantId);
        return NoContent();
    }

    [HttpGet("{configurationId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentModerationFlagDto>))]
    public async Task<IActionResult> GetConfigurationModerationFlags(Guid networkId, Guid configurationId)
    {
        var flags = await moderationFlagService.GetForContentAsync(ContentKind.Configuration, configurationId);
        return Ok(mapper.Map<IEnumerable<ContentModerationFlagDto>>(flags));
    }

    [HttpPost("{configurationId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateConfiguration)]
    [ProducesResponseType(201, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> CreateConfigurationModerationFlag(Guid networkId, Guid configurationId, [FromBody] CreateContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.CreateAsync(networkId, ContentKind.Configuration, configurationId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }

    [HttpPut("{configurationId:guid}/moderation-flags/{flagId:guid}/resolve")]
    [RequirePermission(Constants.Permissions.ModerateConfiguration)]
    [ProducesResponseType(200, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> ResolveConfigurationModerationFlag(Guid networkId, Guid configurationId, Guid flagId, [FromBody] ResolveContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.ResolveAsync(networkId, flagId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }
}