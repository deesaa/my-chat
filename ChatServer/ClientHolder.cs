using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using ChatServer.ServerCommands;
using JsonMessage;

namespace ChatServer;

public class ClientHolder
{
    private Server _server;
    //private string? _name;
    private Guid _id;
    private TcpClient _tcpClient;
    private JsonNetStream _jsonStream;
    
    private int _bufferSize = 8192;

    public bool IsConnected => _tcpClient != null && _tcpClient.Client != null;
    public Guid Id => _id;
   // public string Username => _name;
    public bool IsAuthorized { get; set; }

    public ClientHolder(Guid id, Server server)
    {
        _id = id;
        _server = server;
    }

    public void Connect(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _tcpClient.ReceiveBufferSize = _bufferSize;
        _tcpClient.SendBufferSize = _bufferSize;
        _jsonStream = new JsonNetStream(_tcpClient);
        _jsonStream.OnNext = RouteMessageFromClient;
        _jsonStream.OnDisconnect = Disconnect;
        _jsonStream.BeginRead();
    }

    public void SendMessage(Command command)
    {
        if(!IsConnected)
            return;

        var jsonMessage = command.ToJson();
        _jsonStream.Write(jsonMessage);
        Console.WriteLine($"Message sent from server to client id : {_id}, message : {command}");
    }

    
    
    private void RouteMessageFromClient(JsonNode message)
    {
        Console.WriteLine($"User {_id} : Server receive message: {message}");

        var enterName = message["enterName"];
        var enterPassword = message["enterPassword"];
        if (enterName != null && enterPassword != null)
        {
            string name = enterName.GetValue<string>();
            string password = enterPassword.GetValue<string>();
            _server.OnClientTryEnterNamePassword(_id, name, password);
            return;
        }

        var sendMessageHistoryCount = message["sendMessageHistoryCount"];
        if (sendMessageHistoryCount != null)
        {
            _server.SendMessageHistoryToClient(_id, sendMessageHistoryCount.GetValue<int>());
            return;
        }
        
        var sendUsersData = message["sendUsersData"];
        if (sendUsersData != null)
        {
            _server.SendUsersDataToClient(_id);
            return;
        }

        var messageBody = message["message"]?.GetValue<string>();
        _server.OnMessageFromClient(_id, messageBody ?? "_empty");
    }

    private void Disconnect()
    {
        _jsonStream.Close();
        _tcpClient.Close();
        _server.OnClientLeft(_id);
    }
}