namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IWebsocketService
{
    Task ProcessWebSocketRequestAsync(string groupId, HttpContext context);
}