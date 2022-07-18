using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatClient.FromServerMessageRouts;
using ChatServer;
using JsonMessage;


namespace ChatClient;

public class ChatConnection
{
    private string _ip = "127.0.0.1";
    private int _port = 26950;
    private int _bufferSize = 8192;
    private TcpClient _tcpClient;
    private JsonNetStream _jsonStream;
    private byte[] _receiveBuffer;
    private ChatConfiguration _chatConfiguration;
    private IChatListener _chatListener;
    private List<IServerMessageRout> _serverMessageRouts = ServerMessageRoutsFactory.GetDefaultServerMessageRouts();
    public bool IsAuthorized { get; private set; } = false;
    public bool TryingAuth { get; set; }

    public ChatConnection(ChatConfiguration configuration)
    {
        _chatConfiguration = configuration;
        _tcpClient = new TcpClient()
        {
            ReceiveBufferSize = _bufferSize,
            SendBufferSize = _bufferSize
        };

        _jsonStream = new JsonNetStream(_tcpClient);
        _jsonStream.OnNext = RouteMessageFromServer;
        _jsonStream.OnDisconnect = ServerDisconnected;
    }

    public void BeginConnect() => 
        _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
    
    private void ServerDisconnected()
    {
        IsAuthorized = false;
        _tcpClient.Client.Disconnect(true);
        _chatListener.OnServerDisconnected();
    }

    public bool TryEnterNamePass(string name, string password)
    {
        bool allValid = true;
        allValid &= _chatConfiguration.SanitizeValidate(name, nameof(name), out string outname);
        allValid &= _chatConfiguration.SanitizeValidate(password, nameof(password), out string outpassword);
        if (!allValid) return false;
        
        TryingAuth = true;
        
        var messageObject = JsonSerializer.Serialize(new
        {
            enterName = outname,
            enterPassword = outpassword
        });
        SendMessage(messageObject);
        return true;
    }
    
    public void WriteMessage(string message)
    {
        if(!_chatConfiguration.SanitizeValidate(message, nameof(message), out string outmessage))
            return;
        message = outmessage;
        
        var messageObject = JsonSerializer.Serialize(new
        {
            message = message
        });
        SendMessage(messageObject);
    }
    
    public void RequestLastMessages(int messagesCount)
    {
        var message = SendClientMessageHistoryRout.BuildRequest(messagesCount);
        SendMessage(message);
    }

    public void ChangeTextColor(string newColor)
    {
        var messageObject = JsonSerializer.Serialize(new
        {
            setTextColor = newColor
        });
        SendMessage(messageObject);
    }
    
    public void RequestUsersOnServerData()
    {
        var messageObject = JsonSerializer.Serialize(new
        {
            sendUsersData = true
        });
        SendMessage(messageObject);
    }

    private void RouteMessageFromServer(JsonNode messageJson)
    {
        foreach (var messageRout in _serverMessageRouts)
        {
            if (messageRout.TryRout(_chatListener, messageJson))
            {
                if (messageRout is WrongNameOrPassRout)
                    TryingAuth = false;
                if (messageRout is LoginSuccessRout)
                {
                    TryingAuth = false;
                    IsAuthorized = true;
                }
                break;
            }
        }
    }

    private void SendMessage(string message)
    {
        if(!_tcpClient.Connected)
            return;
        
        try
        {
            _jsonStream.Write(message);
        }
        catch (IOException e)
        {
            ServerDisconnected();
        }
    }
    
    private void OnTcpConnect(IAsyncResult ar)
    {
        try
        {
            _tcpClient.EndConnect(ar);
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("Trying to connect...");
            Thread.Sleep(1500);
            _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
        }

        if(!_tcpClient.Connected)
            return;
        
        _chatListener.OnServerConnected();
        _jsonStream.BeginRead();
    }

    public void AddListener(IChatListener chatListener)
    {
        _chatListener = chatListener;
    }
}