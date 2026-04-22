using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

namespace thirdpartynetwork.Application.Helpers;

public class WebSocketConnection : IDisposable
{
    public string ConnectionId { get; }
    public WebSocket Socket { get; }

    private readonly Channel<byte[]> _sendQueue;
    private readonly CancellationTokenSource _cts = new();

    public WebSocketConnection(string connectionId, WebSocket socket)
    {
        ConnectionId = connectionId;
        Socket = socket;

        _sendQueue = Channel.CreateUnbounded<byte[]>();
        _ = ProcessSendQueueAsync();
    }

    public void EnqueueMessage(byte[] message)
    {
        _sendQueue.Writer.TryWrite(message);
    }

    private async Task ProcessSendQueueAsync()
    {
        try
        {
            await foreach (var message in _sendQueue.Reader.ReadAllAsync(_cts.Token))
            {
                if (Socket.State == WebSocketState.Open)
                {
                    await Socket.SendAsync(
                        new ArraySegment<byte>(message),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
        }
        catch (OperationCanceledException)
        {
            /* Expected when connection closes */
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send loop error for {ConnectionId}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}


public class WebsocketHandler
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocketConnection>> _groups = new();

    public async Task HandleConnectionAsync(string groupId, HttpContext context)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        using var connection = new WebSocketConnection(connectionId, webSocket);

        var group = _groups.GetOrAdd(groupId, _ => new ConcurrentDictionary<string, WebSocketConnection>());
        group.TryAdd(connectionId, connection);

        var joinMessage = Encoding.UTF8.GetBytes($"System: User {connectionId} joined.");

        BroadcastToGroup(groupId, joinMessage);

        await ReceiveLoopAsync(groupId, connection);
    }

    private async Task ReceiveLoopAsync(string groupId, WebSocketConnection connection)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (connection.Socket.State == WebSocketState.Open)
            {
                using var memoryStream = new MemoryStream();
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult =
                        await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await connection.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected",
                            CancellationToken.None);
                        return;
                    }

                    memoryStream.Write(buffer, 0, receiveResult.Count);
                } while (!receiveResult.EndOfMessage);

                var completeMessage = memoryStream.ToArray();
                BroadcastToGroup(groupId, completeMessage);
            }
        }
        finally
        {
            if (_groups.TryGetValue(groupId, out var group))
            {
                group.TryRemove(connection.ConnectionId, out _);

                if (!group.IsEmpty)
                {
                    var leaveMessage = Encoding.UTF8.GetBytes($"System: User {connection.ConnectionId} disconnected.");
                    BroadcastToGroup(groupId, leaveMessage);
                }

                if (group.IsEmpty)
                {
                    _groups.TryRemove(groupId, out _);
                }
            }
        }
    }

    private void BroadcastToGroup(string groupId, byte[] message)
    {
        if (!_groups.TryGetValue(groupId, out var group)) return;

        foreach (var client in group.Values)
        {
            client.EnqueueMessage(message);
        }
    }
}
