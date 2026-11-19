using thirdpartynetwork.Application.Helpers;

namespace thirdpartynetwork.Application.Services;

public class WebsocketService : IWebsocketService
{
    private readonly WebsocketHandler _handler;

    public WebsocketService(WebsocketHandler handler)
    {
        _handler = handler;
    }

    public async Task ProcessWebSocketRequestAsync(string groupId, HttpContext context)
    {
        await _handler.HandleConnectionAsync(groupId, context);
    }
}