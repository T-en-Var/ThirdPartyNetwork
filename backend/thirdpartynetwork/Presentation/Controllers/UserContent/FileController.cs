using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/files")]
[ApiController]
public class FileController(
    IFileService service,
    IContentAccessGrantService accessGrantService,
    IContentViewingLinkService viewingLinkService,
    IContentModerationFlagService moderationFlagService,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadFile, IsListEndpoint = true)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
    public async Task<IActionResult> GetNetworkFiles(Guid networkId)
    {
        var files = await service.GetByNetworkIdAsync(networkId);

        var result = new List<FileDto>();
        foreach (var file in files)
        {
            var dto = mapper.Map<FileDto>(file, opts => { opts.Items["networkId"] = networkId; });
            dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(file, ContentKind.File, Request.Scheme, Request.Host.ToString());
            result.Add(dto);
        }

        return Ok(result);
    }

    [HttpGet("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkFile(Guid networkId, Guid fileId)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();

        var dto = mapper.Map<FileDto>(file, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(file, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageFile, IsCreateEndpoint = true)]
    [ProducesResponseType(201, Type = typeof(FileDto))]
    public async Task<IActionResult> UploadFile(Guid networkId, IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("No file provided.");

        var uploadedFile = await service.UploadFileAsync(networkId, file);
        var dto = mapper.Map<FileDto>(uploadedFile, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(uploadedFile, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPut("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateFile(Guid networkId, Guid fileId, [FromBody] UpdateFileDto updateFile)
    {
        var file = await service.UpdateAsync(networkId, fileId, updateFile);
        var dto = mapper.Map<FileDto>(file, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(file, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpDelete("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFile(Guid networkId, Guid fileId)
    {
        await service.DeleteByIdAsync(networkId, fileId);
        return NoContent();
    }

    [HttpPost("{fileId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishFile(Guid networkId, Guid fileId)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();
        var published = await service.PublishAsync(file);
        var dto = mapper.Map<FileDto>(published, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(published, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost("{fileId:guid}/unpublish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishFile(Guid networkId, Guid fileId)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();
        var unpublished = await service.UnpublishAsync(file);
        var dto = mapper.Map<FileDto>(unpublished, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(unpublished, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpPost("{fileId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetFileOwnershipMode(Guid networkId, Guid fileId, [FromBody] SetOwnershipModeDto setOwnershipModeDto)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(file, setOwnershipModeDto.Mode);
        var dto = mapper.Map<FileDto>(updated, opts => { opts.Items["networkId"] = networkId; });
        dto.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(updated, ContentKind.File, Request.Scheme, Request.Host.ToString());
        return Ok(dto);
    }

    [HttpGet("{fileId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentAccessGrantDto>))]
    public async Task<IActionResult> GetFileAccessGrants(Guid networkId, Guid fileId)
    {
        var grants = await accessGrantService.GetForContentAsync(ContentKind.File, fileId);
        return Ok(mapper.Map<IEnumerable<ContentAccessGrantDto>>(grants));
    }

    [HttpPost("{fileId:guid}/access")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(201, Type = typeof(ContentAccessGrantDto))]
    public async Task<IActionResult> CreateFileAccessGrant(Guid networkId, Guid fileId, [FromBody] CreateContentAccessGrantDto dto)
    {
        var grant = await accessGrantService.CreateAsync(networkId, ContentKind.File, fileId, dto);
        return Ok(mapper.Map<ContentAccessGrantDto>(grant));
    }

    [HttpDelete("{fileId:guid}/access/{grantId:guid}")]
    [RequirePermission(Constants.Permissions.ManageContentAccess, Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RevokeFileAccessGrant(Guid networkId, Guid fileId, Guid grantId)
    {
        await accessGrantService.RevokeAsync(networkId, grantId);
        return NoContent();
    }

    [HttpGet("{fileId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateFile)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ContentModerationFlagDto>))]
    public async Task<IActionResult> GetFileModerationFlags(Guid networkId, Guid fileId)
    {
        var flags = await moderationFlagService.GetForContentAsync(ContentKind.File, fileId);
        return Ok(mapper.Map<IEnumerable<ContentModerationFlagDto>>(flags));
    }

    [HttpPost("{fileId:guid}/moderation-flags")]
    [RequirePermission(Constants.Permissions.ModerateFile)]
    [ProducesResponseType(201, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> CreateFileModerationFlag(Guid networkId, Guid fileId, [FromBody] CreateContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.CreateAsync(networkId, ContentKind.File, fileId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }

    [HttpPut("{fileId:guid}/moderation-flags/{flagId:guid}/resolve")]
    [RequirePermission(Constants.Permissions.ModerateFile)]
    [ProducesResponseType(200, Type = typeof(ContentModerationFlagDto))]
    public async Task<IActionResult> ResolveFileModerationFlag(Guid networkId, Guid fileId, Guid flagId, [FromBody] ResolveContentModerationFlagDto dto)
    {
        var flag = await moderationFlagService.ResolveAsync(networkId, flagId, dto);
        return Ok(mapper.Map<ContentModerationFlagDto>(flag));
    }
}