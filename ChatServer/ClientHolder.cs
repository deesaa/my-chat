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
    private TcpClient _tcpClient;
    private JsonNetStream _jsonStream;
    
    private int _bufferSize = 8192;
    public Guid Id { get; private set; }
    public bool IsAuthorized { get; set; }

    private List<IClientMessageRout> _messageRouts = ClientMessageRoutsFactory.GetDefaultClientMessageRouts();

    public ClientHolder(Guid id, Server server)
    {
        Id = id;
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

    public void Send(Command command)
    {
        var jsonMessage = command.ToJson();
        _jsonStream.Write(jsonMessage);
        Console.WriteLine($"Message sent from server to client id : {Id}, message : {command}");
    }

    private void RouteMessageFromClient(JsonNode message)
    {
        Console.WriteLine($"User {Id} : Server receive message: {message}");

        foreach (var messageRout in _messageRouts)
        {
            if(messageRout.TryRout(Id, _server, message))
                break;
        }
    }

    private void Disconnect()
    {
        _jsonStream.Close();
        _tcpClient.Close();
        _server.OnClientLeft(Id);
    }

    public void SyncClientId(Guid clientId)
    {
        Id = clientId;
    }
}