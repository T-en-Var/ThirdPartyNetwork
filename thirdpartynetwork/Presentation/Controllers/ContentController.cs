using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/content")]
[ApiController]
public class ContentController(IContentService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ViewContent)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentDto>))]
    public IActionResult GetNetworkContent(Guid networkId)
    {
        var content = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<ContentDto>>(content));
    }

    [HttpGet("{contentId:guid}")]
    [RequirePermission(Constants.Permissions.ViewContent)]
    [ProducesResponseType(200, Type = typeof(ContentDto))]
    [ProducesResponseType(404)]
    public IActionResult GetContent(Guid networkId, Guid contentId)
    {
        var content = service.GetById(contentId);
        return content != null ? Ok(mapper.Map<ContentDto>(content)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.CreateContent)]
    [ProducesResponseType(201, Type = typeof(ContentDto))]
    public IActionResult CreateContent(Guid networkId, CreateContentDto createContent)
    {
        var content = service.Create(networkId, createContent);
        return Ok(mapper.Map<ContentDto>(content));
    }

    [HttpPut("{contentId:guid}")]
    [RequirePermission(Constants.Permissions.EditContent)]
    [ProducesResponseType(200, Type = typeof(ContentDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateContent(Guid networkId, Guid contentId, UpdateContentDto updateContent)
    {
        var content = service.Update(networkId, contentId, updateContent);
        return Ok(mapper.Map<ContentDto>(content));
    }

    [HttpDelete("{contentId:guid}")]
    [RequirePermission(Constants.Permissions.DeleteContent)]
    [ProducesResponseType(204)]
    public IActionResult DeleteContent(Guid networkId, Guid contentId)
    {
        service.DeleteById(contentId);
        return NoContent();
    }
}

