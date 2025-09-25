using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/customPages/")]
[ApiController]
public class CustomPageController(ICustomPageService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadCustomPage)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CustomPageDto>))]
    public IActionResult GetNetworkCustomPages(Guid networkId)
    {
        var customPage = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<CustomPageDto>>(customPage));
    }

    [HttpGet("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ReadCustomPage)]
    [ProducesResponseType(200, Type = typeof(CustomPageDto))]
    [ProducesResponseType(404)]
    public IActionResult GetCustomPage(Guid networkId, Guid customPageId)
    {
        var customPage = service.GetById(networkId, customPageId);
        return customPage != null ? Ok(mapper.Map<CustomPageDto>(customPage)) : NotFound();
    }
    
    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(201, Type = typeof(CustomPageDto))]
    public IActionResult CreateCustomPage(Guid networkId, CreateCustomPageDto customPage)
    {
        var uploadedCustomPage = service.Create(networkId, customPage);
        return Ok(mapper.Map<CustomPageDto>(uploadedCustomPage));
    }

    [HttpPut("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(200, Type = typeof(CustomPageDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateCustomPage(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage)
    {
        var customPage = service.Update(networkId, customPageId, updateCustomPage);
        return Ok(mapper.Map<CustomPageDto>(customPage));
    }

    [HttpDelete("{customPageId:guid}")]
    [RequirePermission(Constants.Permissions.ManageCustomPage)]
    [ProducesResponseType(204)]
    public IActionResult DeleteCustomPage(Guid networkId, Guid customPageId)
    {
        service.DeleteById(networkId, customPageId);
        return NoContent();
    }
}
