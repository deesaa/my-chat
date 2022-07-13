// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ChatServer;
using ChatServer;

public static class Bootstrap
{
    private static Server _server;
    static void Main(string[] args)
    {
        Console.Title = "Chat Server";
        _server = new Server(26950);
        Console.ReadKey();
    }
}


public class Server
{
    private int _port;
    private TcpListener _tcpListener;

    private List<Client> _clients = new();
    private List<Message> _messages = new();

    public Server(int port)
    {
        _port = port;

        Console.WriteLine("Starting server...");

        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(OnTcpConnection, null);
        
        Console.WriteLine($"Server started on ip {Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString()}, port {_port}");
    }

    private void OnTcpConnection(IAsyncResult result)
    {
        TcpClient _tcpClient = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(OnTcpConnection, null);

        Client newClient = new Client(Guid.NewGuid(), this);
        _clients.Add(newClient);

        try
        {
            Thread clientThread = new Thread(() => newClient.Connect(_tcpClient));
            clientThread.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        Console.WriteLine("Client connected: " + _tcpClient.Client);
    }

    public void Broadcast(Guid originClientId, string message)
    {
        var originClient = _clients.FirstOrDefault(client => client.Id == originClientId);
        string username = originClient != null ? originClient.Username : "EmptyName"; 
        var messageObject = new Message(originClientId, username, message);
        _messages.Add(messageObject);
        foreach (var client in _clients)
        {
            if(client.Id != originClientId)
                client.SendMessage(messageObject);
        }
    }

    public void OnDisconnected(Guid id)
    {
        _clients.RemoveAll(client => client.Id == id);
        Console.WriteLine("User disconnected : " + id);
    }
}

public class Client
{
    private Server _server;

    private string? _name;
    private Guid _id;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private byte[] _receiveBuffer;
    
    private int _bufferSize = 256;
    private int _maxMessageSize = 128;

    public bool IsConnected => _tcpClient != null && _tcpClient.Client != null;
    public Guid Id => _id;
    public string Username => _name;

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

    public async void SendMessage(Message message)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(message.GetJson());
        await _networkStream.WriteAsync(bytes);
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
            _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
            
            byte[] receivedBytes = new byte[_bufferSize];
            Array.Copy(_receiveBuffer, receivedBytes, _bufferSize);

            string message = Encoding.Unicode.GetString(receivedBytes, 0, byteSize);
            JsonNode messageObject = JsonObject.Parse(message);
            RouteMessageFromClient(messageObject);
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
            return;
        }

        var messageBody = message["message"]?.GetValue<string>();
        _server.Broadcast(_id, messageBody ?? "_empty");
    }

    private void Disconnect()
    {
        _networkStream.Close();
        _tcpClient.Close();
        _server.OnDisconnected(_id);
    }
}

public class Message
{
    private static ulong _lastMessageId;
    public static ulong NextMessageId => _lastMessageId++;

    private Guid _originClientId;
    private string _username;
    private DateTime _utcTime;
    private ulong _messageId;
    private string _message;
    
    public Message(Guid originClientId, string username, string message)
    {
        _username = username;
        _originClientId = originClientId;
        _utcTime = DateTime.UtcNow;
        _messageId = NextMessageId;
        _message = message;
    }

    public string GetJson()
    {
        string json = JsonSerializer.Serialize(new
        {
            utcTime = _utcTime,
            messageId = _messageId,
            message = _message
        });
        return json;
    }
}