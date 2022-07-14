using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;

namespace ChatServer;

public class Client
{
    private Server _server;

    private string? _name;
    private Guid _id;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private byte[] _receiveBuffer;
    
    private int _bufferSize = 8192;
  //  private int _maxMessageSize = 128;

    public bool IsConnected => _tcpClient != null && _tcpClient.Client != null;
    public Guid Id => _id;
    public string Username => _name;
    public bool IsAuthorized => _name != null;

    public Client(Guid id, Server server)
    {
        _id = id;
        _server = server;
    }

    public void Connect(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _tcpClient.ReceiveBufferSize = _bufferSize;
        _tcpClient.SendBufferSize = _bufferSize;
        _networkStream = _tcpClient.GetStream();
        _receiveBuffer = new byte[_bufferSize];
        
        _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
    }

    public void SendMessage(Message message)
    {
        if(!IsConnected)
            return;

        var jsonMessage = message.ToJson();
        byte[] bytes = Encoding.Unicode.GetBytes(jsonMessage);
        int byteSize = bytes.Length;
        _networkStream.Write(BitConverter.GetBytes(byteSize));
        _networkStream.Write(bytes);
        Console.WriteLine($"Message sent from server to client id : {_id}, message : {message}");
    }

    private void OnNetworkStreamData(IAsyncResult receive)
    {
        try
        {
            int byteSize = _networkStream.EndRead(receive);
            if (byteSize <= 0)
            {
                Disconnect();
                return;
            }
            
            byte[] receivedBytes = new byte[_bufferSize];
            Array.Copy(_receiveBuffer, receivedBytes, _bufferSize);

            string message = Encoding.Unicode.GetString(receivedBytes, 0, byteSize);
            JsonNode messageObject = JsonObject.Parse(message);
            RouteMessageFromClient(messageObject);
            
            _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);

        }
        catch (Exception e)
        { 
            Console.WriteLine("OnNetworkStreamData Exception: " + e);
        }
    }

    private void RouteMessageFromClient(JsonNode message)
    {
        Console.WriteLine($"User {_id} : Server receive message: {message}");

        var setName = message["setName"];
        if (setName != null)
        {
            _name = setName.GetValue<string>();
            _server.OnClientEnterNameAndJoin(_id);
            return;
        }

        var sendMessageHistoryCount = message["sendMessageHistoryCount"];
        if (sendMessageHistoryCount != null)
        {
            _server.SendMessageHistoryToClient(_id, sendMessageHistoryCount.GetValue<int>());
            return;
        }

        var messageBody = message["message"]?.GetValue<string>();
        _server.OnMessageFromClient(_id, messageBody ?? "_empty");
    }

    private void Disconnect()
    {
        _networkStream.Close();
        _tcpClient.Close();
        _server.OnDisconnected(_id);
    }
}