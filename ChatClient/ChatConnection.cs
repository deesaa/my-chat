using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonMessage;
using JsonMessage.DTO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

    private bool _isAuthorized = false;
    public bool IsAuthorized => _isAuthorized;

    public Action<MessageDto> OnMessageFromServer;
    public Action OnServerConnected;
    public Action OnServerDisconnected;
    public Action<UserDto[]> OnUsersOnServerList;
    public Action<string> OnOtherClientConnected;
    public Action<string> OnOtherClientDisconnected;
    public Action<string, string> OnUserNameColorChanged;
    public Action OnLoginSuccess;
    public Action OnLoginFail;

    
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
    }

    public void BeginConnect() => 
        _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
    
    public bool Connected => _tcpClient.Connected;

    public void EnterNamePass(string name, string password)
    {
        if(!_chatConfiguration.SterilizeValidate(name, nameof(name), out string outname))
            return;
        name = outname;
        if(!_chatConfiguration.SterilizeValidate(password, nameof(password), out string outpassword))
            return;
        password = outpassword;
        
        var messageObject = JsonSerializer.Serialize(new
        {
            enterName = name,
            enterPassword = password
        });
        SendMessage(messageObject);
    }
    
    public void Write(string message)
    {
        if(!_chatConfiguration.SterilizeValidate(message, nameof(message), out string outmessage))
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
        var messageObject = JsonSerializer.Serialize(new
        {
            sendMessageHistoryCount = messagesCount
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
        var messageObject = messageJson.AsObject();

        if (messageObject.TryGetPropertyValue("userJoinedServer", out var userJoinedName))
        {
            var jsonString = messageObject["user"].ToJsonString();
            UserDto user = JsonConvert.DeserializeObject<UserDto>(jsonString);
            OnOtherClientConnected(user.Username);
            return;
        }

        if (messageObject.TryGetPropertyValue("userLeftName", out var userLeftName))
        {
            var name = userLeftName.GetValue<string>();
            OnOtherClientDisconnected(name);
            return;
        }
        
        if (messageObject.TryGetPropertyValue("loginSuccess", out var loginSuccess))
        {
            _isAuthorized = true;
            OnLoginSuccess();
            return;
        }
        
        if (messageObject.TryGetPropertyValue("wrongNamePass", out var wrongNamePass))
        {
            OnLoginFail();
            return;
        }
        
        if (messageObject.TryGetPropertyValue("userSetNameColor", out var userSetNameColor))
        {
            var newNameColor = userSetNameColor.GetValue<string>();
            var usernameWithNewColor = messageObject["username"].GetValue<string>();
            OnUserNameColorChanged(usernameWithNewColor, newNameColor);
            return;
        }
        
        if (messageObject.TryGetPropertyValue("usersOnServer", out var usersOnServer))
        {
            var jsonArray = usersOnServer.AsArray();
            if(jsonArray.Count <= 0)
                return;

            var jsonString = usersOnServer.ToJsonString();
            UserDto[] users = JsonConvert.DeserializeObject<UserDto[]>(jsonString);
            
            OnUsersOnServerList(users);
            return;
        }
        
        
        var json = messageObject["userMessage"].ToJsonString();
        var messageData = JsonConvert.DeserializeObject<MessageDto>(json);
        OnMessageFromServer(messageData);
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

    private void ServerDisconnected()
    {
        ResetConnection();
        OnServerDisconnected();
    }

    private void ResetConnection()
    {
        _isAuthorized = false;
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
        
        OnServerConnected();
        _jsonStream.BeginRead();
    }
}