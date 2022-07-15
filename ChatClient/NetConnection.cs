using System.Net.Sockets;
using System.Text.Json.Nodes;
using JsonMessage;

namespace ChatClient;

public abstract class NetConnection
{
    private string _ip = "127.0.0.1";
    private int _port = 26950;

    private int _bufferSize = 8192;

    private TcpClient _tcpClient;
    private JsonNetStream _jsonStream;
    private byte[] _receiveBuffer;
    
    public NetConnection()
    {
        _tcpClient = new TcpClient()
        {
            ReceiveBufferSize = _bufferSize,
            SendBufferSize = _bufferSize
        };

        _jsonStream = new JsonNetStream(_tcpClient);
        _jsonStream.OnNext = OnMessage;
    }

    protected abstract void OnMessage(JsonObject message);
}