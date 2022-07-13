using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public static class Bootstrap
{
    static void Main(string[] args)
    {
        ChatConnection client = new ChatConnection();

        client.OnMessageFromServer = messageObject =>
        {
            var dateTime = messageObject["utcTime"].GetValue<DateTime>();
            var message = messageObject["message"].GetValue<string>();
            var messageId = messageObject["messageId"].GetValue<int>();
            Console.WriteLine($"{message} ({dateTime.ToLocalTime().ToShortTimeString()})");
        };

        client.SetMessageValidator(ValidatorFactory.GetDefaultMessageValidator());
        client.SetNameValidator(ValidatorFactory.GetDefaultNameValidator());
        client.SetMessageSterilizer(new TrimSterilizer());
        client.SetNameSterilizer(new EmptyCharsSterilizer());

        while (!client.IsNameSet)
        {
            Console.WriteLine("Enter Name:");
            string name = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack();
            client.SetName(name);
        }

        while (true)
        {
            Console.WriteLine("Enter Message:");
            string message = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack(2);
            client.Write(message);
        }
    }
}

public static class Ext
{
    public static void ConsoleLineBack(int linesCount = 1)
    {
        
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
    }
}

public static class ValidatorFactory
{
    public static IValidator GetDefaultNameValidator()
    {
       return new LengthValidator(4, 16,
            (name, maxLength) =>
                Console.WriteLine($"Name {name} is too long. Max size is {maxLength} chars"),
            (name, minLength) =>
                Console.WriteLine($"Name {name} is too small. Min size is {minLength} chars"));
    }
    
    public static IValidator GetDefaultMessageValidator()
    {
        return new LengthValidator(1, 128,
            (message, maxLength) => 
                Console.WriteLine($"Message {message} is too long. Max size is {maxLength} chars"),
            (message, minLength) =>
                Console.WriteLine($"Message {message} is too small. Min size is {minLength} chars"));
    }
}

public interface IValidator
{
    bool Validate(string value);
}

public class TrimSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value.Trim();
    }
}

public class EmptyCharsSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value.Replace(" ", "");
    }
}

public class NullSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value;
    }
}

public interface ISterilizer
{
    string Sterilize(string value);
}

public class NullValidator : IValidator
{
    public bool Validate(string value)
    {
        return true;
    }
}

public class LengthValidator : IValidator
{
    private Action<string, int> _onToLongCallback;
    private Action<string, int> _onToSmallCallback;
    private int _minLength;
    private int _maxLength;
    
    public LengthValidator(int minLength, int maxLength, 
        Action<string, int> onToLongCallback, Action<string, int> onToSmallCallback)
    {
        _onToLongCallback = onToLongCallback;
        _onToSmallCallback = onToSmallCallback;
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public bool Validate(string value)
    {
        if (value.Length < _minLength)
        {
            _onToSmallCallback(value, _minLength);
            return false;
        }
        if (value.Length > _maxLength)
        {
            _onToLongCallback(value, _maxLength);
            return false;
        }
        return true;
    }
}

public class ChatConnection
{
    private string _ip = "127.0.0.1";
    private int _port = 26950;

    private int _bufferSize = 256;

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private byte[] _receiveBuffer;

    private bool _isNameSet;

    public Action OnServerConnected;
    public Action<JsonNode> OnMessageFromServer;
    public Action OnServerDisconnected;

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
        _receiveBuffer = new byte[_bufferSize];
        _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
    }

    public bool IsNameSet => _isNameSet;

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
        byte[] messageBytes = Encoding.Unicode.GetBytes(message);
        _networkStream.Write(messageBytes);
    }

    private void OnTcpConnect(IAsyncResult ar)
    {
        _tcpClient.EndConnect(ar);
        
        if(!_tcpClient.Connected)
            return;

        _networkStream = _tcpClient.GetStream();
        _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
    }

    private void OnNetworkStreamData(IAsyncResult ar)
    {
        try
        {
            int byteSize = _networkStream.EndRead(ar);
            if (byteSize <= 0)
            {
                return;
            }
            _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);

            
            byte[] _data = new byte[byteSize];
            Array.Copy(_receiveBuffer, _data, byteSize);
            string message = Encoding.Unicode.GetString(_data);
            var jsonMessage = JsonObject.Parse(message);
            RouteMessageFromServer(jsonMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void RouteMessageFromServer(JsonNode message)
    {
        OnMessageFromServer(message);
    }
}