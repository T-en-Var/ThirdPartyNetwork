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
        var result = mapper.Map<IEnumerable<FileDto>>(files, opts => { opts.Items["networkId"] = networkId; });
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
        return Ok(mapper.Map<FileDto>(uploadedFile, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpPut("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateFile(Guid networkId, Guid fileId, [FromBody] UpdateFileDto updateFile)
    {
        var file = await service.UpdateAsync(networkId, fileId, updateFile);
        return Ok(mapper.Map<FileDto>(file, opts => { opts.Items["networkId"] = networkId; }));
    }

    [HttpDelete("{fileId:guid}")]
    [RequirePermission(Constants.Permissions.ManageFile)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFile(Guid networkId, Guid fileId)
    {
        await service.DeleteByIdAsync(networkId, fileId);
        return NoContent();
    }

    // PLAN.md §6: replaces the old "set AccessLevel" update path.
    [HttpPost("{fileId:guid}/publish")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishFile(Guid networkId, Guid fileId)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();
        var published = await service.PublishAsync(file);
        return Ok(mapper.Map<FileDto>(published, opts => { opts.Items["networkId"] = networkId; }));
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
        return Ok(mapper.Map<FileDto>(unpublished, opts => { opts.Items["networkId"] = networkId; }));
    }

    // PLAN.md §2.1/§4: without this, OwnershipMode had no write path at all.
    [HttpPost("{fileId:guid}/ownership")]
    [RequirePermission(Constants.Permissions.IsContentOwner)]
    [ProducesResponseType(200, Type = typeof(FileDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetFileOwnershipMode(Guid networkId, Guid fileId, [FromBody] SetOwnershipModeDto dto)
    {
        var file = await service.GetByIdAsync(networkId, fileId);
        if (file == null) return NotFound();
        var updated = await service.SetOwnershipModeAsync(file, dto.Mode);
        return Ok(mapper.Map<FileDto>(updated, opts => { opts.Items["networkId"] = networkId; }));
    }

    // PLAN.md §2.4/§6: sharing/grant CRUD — one shared ContentAccessGrantService backs all
    // five content controllers' thin methods here.
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

    // PLAN.md §2.5/§6: moderators flag/hide, they never mutate (Q2) — gated by ModerateFile,
    // never IsContentOwner (this is deliberately NOT an author/owner action).
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