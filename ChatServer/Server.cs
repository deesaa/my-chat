using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using ChatServer.Database;
using ChatServer.ServerCommands;
using JsonMessage.DTO;

namespace ChatServer;

public class Server
{
    private int _port;
    private TcpListener _tcpListener;
    private List<ClientHolder> _clients = new();
    private IChatDb _chatDb;
    private ISaveLoad _chatDbSaveLoad;

    public Server(int port)
    {
        _port = port;
        _chatDb = new ChatDb();
        _chatDbSaveLoad = new JsonDbSaveLoad(_chatDb, "chatDb.txt");
        _chatDbSaveLoad.Load();

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
    
    private void Broadcast(Command command, bool excludeSelf = true)
    {
        foreach (var client in _clients)
        {
            if((client.Id != command.OriginId || !excludeSelf) && client.IsAuthorized)
                client.Send(command);
        }
    }
    
    public void OnMessageFromClient(Guid clientId, string message)
    {
        UserData? userData = _chatDb.GetUserData(clientId);
        if (userData != null)
        {
            MessageCommand messageCommand = new MessageCommand(userData.Value, message);
            MessageData data = messageCommand.CreateData();
            _chatDb.AddMessage(data);
            Broadcast(messageCommand, false);
        }
    }
    
    public void OnClientTryEnterNamePassword(Guid clientId, string name, string password)
    {
        var client = _clients.FirstOrDefault(client => client.Id == clientId);
        if(client == null) return;
        
        if (_chatDb.GetAllUsers().TryFirst(out var outuser, user => user.Username == name))
        {
            if (outuser.Password != password)
            {
                client.Send(new WrongNameOrPasswordCommand(clientId));
                return;
            }
            client.SyncClientId(outuser.Id);
            client.Send(new LoginSuccessCommand(outuser.Id));
            _chatDb.SetUserOnline(outuser, true);
            client.IsAuthorized = true;
            Broadcast(new UserJoinedCommand(outuser));
            return;
        }

        var newUser = new UserData(clientId, name, password);
        _chatDb.AddUserData(newUser);
        client.Send(new LoginSuccessCommand(newUser.Id));
        client.IsAuthorized = true;
        Broadcast(new UserJoinedCommand(newUser));
    }
    
    public void OnClientLeft(Guid clientId)
    {
        var user = _chatDb.GetUserData(clientId).Value;
        _chatDb.SetUserOnline(user, false);
        var messageObject = new UserLeftCommand(clientId, user.Username);
        Broadcast(messageObject);
        _clients.RemoveAll(client => client.Id == clientId);
        Console.WriteLine("User disconnected : " + clientId);
    }

    public void SendMessageHistoryToClient(Guid clientId, int sendMessageHistoryCount = 10)
    {
        var client = _clients.FirstOrDefault(client => client.Id == clientId);
        if(client == null)
            return;
        
        foreach (var messageData in _chatDb.GetAllMessages().TakeLast(sendMessageHistoryCount))
        {
            UserData user = _chatDb.GetUserData(messageData.SenderUserId).Value;
            client.Send(new MessageCommand(messageData, user));
        }
    }

    public void SendUsersDataToClient(Guid clientId)
    {
        var client = _clients.FirstOrDefault(client => client.Id == clientId);
        if(client == null)
            return;
        client.Send(new SendUsersDataCommand(clientId, _chatDb.GetAllUsers()));
    }
    
    public void SetTextColorForUser(Guid clientId, string color)
    {
        UserData user = _chatDb.GetUserData(clientId).Value;
        _chatDb.SetUserColor(user, color);
        Broadcast(new UserChangedTextColorCommand(clientId, user.Username, color), false);
    }
    
    public void OnQuit()
    {
        _chatDbSaveLoad.Save();
    }
}