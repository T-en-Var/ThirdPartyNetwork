using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/files")]
[ApiController]
public class FileController(IFileService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadFile)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
    public IActionResult GetNetworkFile(Guid networkId)
    {
        var file = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<FileDto>>(file));
    }

    [HttpGet("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public IActionResult GetFile(Guid networkId, Guid fileId)
    {
        var file = service.GetById(networkId, fileId);
        return file != null ? Ok(mapper.Map<FileDto>(file)) : NotFound();
    }
    
    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(201, Type = typeof(FileDto))]
    public async Task<IActionResult> UploadFile(Guid networkId, IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("No file provided.");

        var uploadedFile = await service.UploadFileAsync(networkId, file);
        return Ok(mapper.Map<FileDto>(uploadedFile));
    }

    [HttpPut("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateFile(Guid networkId, Guid fileId, UpdateFileDto updateFile)
    {
        var file = service.Update(networkId, fileId, updateFile);
        return Ok(mapper.Map<FileDto>(file));
    }

    [HttpDelete("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFile(Guid networkId, Guid fileId)
    {
        await service.DeleteByIdAsync(networkId, fileId);
        return NoContent();
    }
}
