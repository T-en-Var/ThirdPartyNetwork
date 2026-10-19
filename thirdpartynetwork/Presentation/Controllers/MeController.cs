using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("me")]
[ApiController]
public class MeController(IMeService service, INetworkService networkService, IMapper mapper, IContentViewingLinkService viewingLinkService) : ControllerBase
{
    [HttpGet("")]
    [ProducesResponseType(200, Type = typeof(UserProxyExtraCascadeDto))]
    [ProducesResponseType(401)]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userProxy = await service.GetMeAsync();
        var mainNetwork = await networkService.GetMainNetworkAsync(); // what about external networks?
        
        var userProxyDto = mapper.Map<UserProxyDto>(userProxy, opts => opts.Items["networkId"] = mainNetwork.Id);
        if (userProxy?.ImageFile != null)
        {
            userProxyDto.ImageFile!.ViewingUrl = await viewingLinkService.ComputeViewingUrlAsync(userProxy.ImageFile,
                ContentKind.File, Request.Scheme, Request.Host.ToString());
        }
        return Ok(userProxyDto);
    }
    
    [HttpGet("permissions")]
    [ProducesResponseType(200, Type = typeof(ICollection<NetworkPermissionsCollectionDto>))]
    [ProducesResponseType(401)]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions()
    {
        var permissionsCollection = await service.GetMyPermissionsAsync();
        return Ok(permissionsCollection);
    }
}
