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
        return customPage != null ? Ok(mapper.Map<PageDto>(customPage, opts => { opts.Items["networkId"] = networkId; })) : NotFound();
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
}