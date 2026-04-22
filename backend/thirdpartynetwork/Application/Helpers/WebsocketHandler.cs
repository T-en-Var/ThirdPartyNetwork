using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace thirdpartynetwork.Application.Helpers;

public class WebsocketHandler
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> _groups = new();

    public async Task HandleConnectionAsync(string groupId, HttpContext context)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        var group = _groups.GetOrAdd(groupId, _ => new ConcurrentDictionary<string, WebSocket>());
        group.TryAdd(connectionId, webSocket);

        var joinText = $"System: User {connectionId} joined.";
        var joinMessage = Encoding.UTF8.GetBytes(joinText);
        
        await BroadcastToGroupAsync(
            groupId, 
            connectionId, 
            joinMessage, 
            joinMessage.Length, 
            WebSocketMessageType.Text, 
            true
        );

        await ReceiveLoopAsync(groupId, connectionId, webSocket);
    }
    
    private async Task ReceiveLoopAsync(string groupId, string connectionId, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                Console.WriteLine("Receiving message");
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Console.WriteLine($"Received {receiveResult.Count} bytes");

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                    break;
                }

                await BroadcastToGroupAsync(groupId, connectionId, buffer, receiveResult.Count, receiveResult.MessageType, receiveResult.EndOfMessage);
            }
        }
        finally
        {
            if (_groups.TryGetValue(groupId, out var group))
            {
                group.TryRemove(connectionId, out _);
                
                if (!group.IsEmpty) 
                {
                    var leaveText = $"System: User {connectionId} disconnected.";
                    var leaveMessage = Encoding.UTF8.GetBytes(leaveText);
                    
                    await BroadcastToGroupAsync(
                        groupId, 
                        connectionId, 
                        leaveMessage, 
                        leaveMessage.Length, 
                        WebSocketMessageType.Text, 
                        true
                    );
                }
                
                if (group.IsEmpty)
                {
                    _groups.TryRemove(groupId, out _);
                }
            }
        }
    }

    private async Task BroadcastToGroupAsync(string groupId, string senderConnectionId, byte[] buffer, int count, WebSocketMessageType messageType, bool endOfMessage)
    {
        if (!_groups.TryGetValue(groupId, out var group)) return;

        var messageSegment = new ArraySegment<byte>(buffer, 0, count);

        // Create a copy of the clients to avoid modification issues during iteration
        var clients = group.Values.Where(ws => ws.State == WebSocketState.Open).ToList();

        foreach (var client in clients)
        {
            try 
            {
                // Note: In a production app, you'd want a SemaphoreSlim per connection 
                // to ensure this specific 'client' isn't currently sending something else.
                await client.SendAsync(messageSegment, messageType, endOfMessage, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending to a client: {ex.Message}");
            }
        }
    }
}