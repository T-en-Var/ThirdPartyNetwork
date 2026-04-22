using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Controllers;

[ApiController]
[Route("websocket")]
public class WebsocketController : ControllerBase
{
    private readonly IWebsocketService _websocketService;

    public WebsocketController(IWebsocketService websocketService)
    {
        _websocketService = websocketService;
    }

    [HttpGet("group/{groupId}")]
    public async Task Get(string groupId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await _websocketService.ProcessWebSocketRequestAsync(groupId, HttpContext);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}