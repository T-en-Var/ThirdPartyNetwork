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
    public IActionResult GetNetworkConfigurations(Guid networkId)
    {
        var list = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<ConfigurationDto>>(list));
    }

    [HttpGet("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ReadConfiguration)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public IActionResult GetConfiguration(Guid networkId, Guid configurationId)
    {
        var cfg = service.GetById(networkId, configurationId);
        return cfg != null ? Ok(mapper.Map<ConfigurationDto>(cfg)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(201, Type = typeof(ConfigurationDto))]
    public IActionResult CreateConfiguration(Guid networkId, [FromBody] CreateConfigurationDto configuration)
    {
        var created = service.Create(networkId, configuration);
        return CreatedAtAction(nameof(GetConfiguration), new { networkId = networkId, configurationId = created.Id },
            mapper.Map<ConfigurationDto>(created));
    }

    [HttpPut("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(200, Type = typeof(ConfigurationDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateConfiguration(Guid networkId, Guid configurationId,
        [FromBody] UpdateConfigurationDto updateConfiguration)
    {
        var updated = service.Update(networkId, configurationId, updateConfiguration);
        return Ok(mapper.Map<ConfigurationDto>(updated));
    }

    [HttpDelete("{configurationId:guid}")]
    [RequirePermission(Constants.Permissions.ManageConfiguration)]
    [ProducesResponseType(204)]
    public IActionResult DeleteConfiguration(Guid networkId, Guid configurationId)
    {
        service.DeleteById(networkId, configurationId);
        return NoContent();
    }
}