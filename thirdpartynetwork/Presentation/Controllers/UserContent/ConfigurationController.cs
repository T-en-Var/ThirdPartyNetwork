using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/configurations")]
[ApiController]
public class ConfigurationController(IConfigurationService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ConfigurationDto>))]
    public async Task<IActionResult> GetNetworkConfigurations(Guid networkId)
    {
        var list = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<ConfigurationDto>>(list));
    }

    [HttpGet("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfiguration(Guid networkId, Guid configurationId)
    {
        var cfg = await service.GetByIdAsync(networkId, configurationId);
        return cfg != null ? Ok(mapper.Map<ConfigurationDto>(cfg)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(201, Type = typeof(ConfigurationDto))]
    public async Task<IActionResult> CreateConfiguration(Guid networkId, [FromBody] CreateConfigurationDto configuration)
    {
        var created = await service.CreateAsync(networkId, configuration);
        return CreatedAtAction(nameof(GetConfiguration), new { networkId = networkId, configurationId = created.Id },
            mapper.Map<ConfigurationDto>(created));
    }

    [HttpPut("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateConfiguration(Guid networkId, Guid configurationId,
        [FromBody] UpdateConfigurationDto updateConfiguration)
    {
        var updated = await service.UpdateAsync(networkId, configurationId, updateConfiguration);
        return Ok(mapper.Map<ConfigurationDto>(updated));
    }

    [HttpDelete("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteConfiguration(Guid networkId, Guid configurationId)
    {
        await service.DeleteByIdAsync(networkId, configurationId);
        return NoContent();
    }
}