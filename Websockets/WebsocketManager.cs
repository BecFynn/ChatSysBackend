using System.Text;
using System.Net.WebSockets;

public class WebsocketManager
{
    private readonly List<WebSocket> _sockets = new();

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }

    public async Task BroadcastAsync(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes);

        foreach (var socket in _sockets.ToList())
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public void RemoveClosedSockets()
    {
        _sockets.RemoveAll(s => s.State != WebSocketState.Open);
    }
}