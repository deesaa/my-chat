using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatServer;
using JsonMessage;

public class ChatConnection
{
    private string _ip = "127.0.0.1";
    private int _port = 26950;

    private int _bufferSize = 8192;

    private TcpClient _tcpClient;
    private JsonNetStream _jsonStream;
    private byte[] _receiveBuffer;

    private bool _isNameSet;

    public Action<MessageData> OnMessageFromServer;
    public Action OnServerConnected;
    public Action OnServerDisconnected;
    public Action<string> OnOtherClientConnected;
    public Action<string> OnOtherClientDisconnected;
    public Action<string, string> OnUserNameColorChanged;

    private IValidator _nameValidator = new NullValidator();
    private IValidator _messageValidator = new NullValidator();
    private ISterilizer _nameSterilizer = new NullSterilizer(); 
    private ISterilizer _messageSterilizer = new NullSterilizer(); 
    public void SetNameValidator(IValidator validator) => _nameValidator = validator;
    public void SetMessageValidator(IValidator validator) => _messageValidator = validator;
    public void SetNameSterilizer(ISterilizer sterilizer) => _nameSterilizer = sterilizer;
    public void SetMessageSterilizer(ISterilizer sterilizer) => _messageSterilizer = sterilizer;


    public ChatConnection()
    {
        _tcpClient = new TcpClient()
        {
            ReceiveBufferSize = _bufferSize,
            SendBufferSize = _bufferSize
        };

        _jsonStream = new JsonNetStream(_tcpClient);
        _jsonStream.OnNext = RouteMessageFromServer;
    }

    public void BeginConnect()
    {
        _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
    }

    public bool IsNameSet => _isNameSet;
    public bool Connected => _tcpClient.Connected;

    public void SetName(string name)
    {
        name = _nameSterilizer.Sterilize(name);
        if (!_nameValidator.Validate(name))
            return;

        _isNameSet = true;
        var messageObject = JsonSerializer.Serialize(new
        {
            setName = name
        });
        SendMessage(messageObject);
    }
    
    public void Write(string message)
    {
        message = _messageSterilizer.Sterilize(message);
        if (!_messageValidator.Validate(message))
            return;
        
        var messageObject = JsonSerializer.Serialize(new
        {
            message = message
        });
        SendMessage(messageObject);
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
        _isNameSet = false;
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
    
    public interface IServerMessage
    {
        public void Execute();
    }

    public class UserJoinedMessage : IServerMessage
    {
        private string _userName;
        private Action<string> OnMessage;
        public UserJoinedMessage(string userName, Action<string> onMessage)
        {
            _userName = userName;
            OnMessage = onMessage;
        }

        public void Execute()
        {
            OnMessage(_userName);
        }
    }

    private void RouteMessageFromServer(JsonNode messageJson)
    {
        var messageObject = messageJson.AsObject();

        if (messageObject.TryGetPropertyValue("userJoinedName", out var userJoinedName))
        {
            var name = userJoinedName.GetValue<string>();
            OnOtherClientConnected(name);
            return;
        }

        if (messageObject.TryGetPropertyValue("userLeftName", out var userLeftName))
        {
            var name = userLeftName.GetValue<string>();
            OnOtherClientDisconnected(name);
            return;
        }
        
        if (messageObject.TryGetPropertyValue("userSetNameColor", out var userSetNameColor))
        {
            var newNameColor = userSetNameColor.GetValue<string>();
            var usernameWithNewColor = messageObject["username"].GetValue<string>();
            OnUserNameColorChanged(usernameWithNewColor, newNameColor);
            return;
        }

        var messageData = messageObject["userMessage"].GetValue<MessageData>();
        OnMessageFromServer(messageData);
    }

    public void RequestMessageHistory(int messagesCount)
    {
        var messageObject = JsonSerializer.Serialize(new
        {
            sendMessageHistoryCount = messagesCount
        });
        SendMessage(messageObject);
    }
}
