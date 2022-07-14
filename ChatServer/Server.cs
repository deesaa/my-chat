using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;

namespace ChatServer;

public class Server
{
    private int _port;
    private TcpListener _tcpListener;

    private List<ClientHolder> _clients = new();
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
        
        ClientHolder newClientHolder = new ClientHolder(Guid.NewGuid(), this);
        _clients.Add(newClientHolder);

        try
        {
            Thread clientThread = new Thread(() => newClientHolder.Connect(_tcpClient));
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
        Broadcast(messageObject, false);
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
        var messageObject = new UserJoinedMessage(clientId, username);
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

    public void SendMessageHistoryToClient(Guid clientId, int sendMessageHistoryCount)
    {
        var client = _clients.FirstOrDefault(client => client.Id == clientId);
        if(client == null)
            return;

        List<Message> messages = new List<Message>();
        
        for (int i = 0; i < sendMessageHistoryCount; i++)
        {
            if(_messages.Count - i <= 0)
                break;
            int index = _messages.Count - (i + 1);
            messages.Add(_messages[index]);
        }
        
        messages.Reverse();
        
        foreach (var message in messages)
        {
            client.SendMessage(message);
        }
    }
}