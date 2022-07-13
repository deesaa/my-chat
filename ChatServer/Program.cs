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
    private Guid _serverId;

    public Server(int port)
    {
        _port = port;
        _serverId = Guid.NewGuid();

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
    
    public void OnMessageFromClient(Guid clientId, string message)
    {
        var originClient = _clients.FirstOrDefault(client => client.Id == clientId);
        string username = originClient != null ? originClient.Username : "_NULL_"; 
        var messageObject = new SimpleMessage(clientId, username, message);
        Broadcast(messageObject);
    }

    private void Broadcast(Message message, bool excludeSelf = true)
    {
        _messages.Add(message);
        foreach (var client in _clients)
        {
            if((client.Id != message.SenderClientId || !excludeSelf) && client.IsAuthorized)
                client.SendMessage(message);
        }
    }
    
    public void OnClientEnterNameAndJoin(Guid clientId)
    {
        var originClient = _clients.FirstOrDefault(client => client.Id == clientId);
        string username = originClient != null ? originClient.Username : "EmptyName";
        var messageObject = new UserJoinedMessage(_serverId, username);
        Broadcast(messageObject);
    }
    
    public void OnClientLeft(Guid clientId)
    {
        var originClient = _clients.FirstOrDefault(client => client.Id == clientId);
        string username = originClient != null ? originClient.Username : "EmptyName";
        var messageObject = new UserLeftMessage(clientId, username);
        Broadcast(messageObject);
    }

    public void OnDisconnected(Guid clientId)
    {
        OnClientLeft(clientId);
        
        _clients.RemoveAll(client => client.Id == clientId);
        Console.WriteLine("User disconnected : " + clientId);
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

    public async void SendMessage(Message message)
    {
        if(!IsConnected)
            return;
        
        byte[] bytes = Encoding.Unicode.GetBytes(message.ToJson());
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
            _server.OnClientEnterNameAndJoin(_id);
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


public abstract class Message
{
    private static ulong _lastMessageId;
    public static ulong NextMessageId => _lastMessageId++;

    private Guid _senderClientId;
    public Guid SenderClientId
    {
        get => _senderClientId;
        protected set
        {
            _senderClientId = SenderClientId;
        }
    }

    protected readonly DateTime UtcTime;
    protected readonly ulong MessageOrdinalId;
    
    public Message(Guid senderClientId)
    {
        SenderClientId = senderClientId;
        UtcTime = DateTime.UtcNow;
        MessageOrdinalId = NextMessageId;
    }
    
    public abstract string ToJson();
}


public class UserJoinedMessage : Message
{
    private string _joinedUsername;

    public UserJoinedMessage(Guid senderClientId, string joinedUsername) : base(senderClientId)
    {
        _joinedUsername = joinedUsername;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userJoinedName = _joinedUsername
        });
    }
}

public class UserLeftMessage : Message
{
    private string _joinedUsername;

    public UserLeftMessage(Guid senderClientId, string joinedUsername) : base(senderClientId)
    {
        _joinedUsername = joinedUsername;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userLeftName = _joinedUsername
        });
    }
}


public class SimpleMessage : Message
{
    private string _username;
    private string _message;
    
    public SimpleMessage(Guid senderClientId, string username, string message) : base(senderClientId)
    {
        _username = username;
        _message = message;
    }

    public override string ToJson()
    {
        string json = JsonSerializer.Serialize(new
        {
            utcTime = UtcTime,
            messageId = MessageOrdinalId,
            message = _message,
            username = _username
        });
        return json;
    }
}