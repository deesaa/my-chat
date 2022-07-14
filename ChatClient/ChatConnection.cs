using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonMessage;

public class ChatConnection
{
    private string _ip = "127.0.0.1";
    private int _port = 26950;

    private int _bufferSize = 8192;

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private MessageStream _messageStream;
    private byte[] _receiveBuffer;

    private bool _isNameSet;

    public Action OnServerConnected;
    public Action<MessageData> OnMessageFromServer;
    public Action OnServerDisconnected;
    public Action<string> OnOtherClientConnected;
    public Action<string> OnOtherClientDisconnected;

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
        try
        {
            _tcpClient.EndConnect(ar);
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("Trying to connect...");
            Thread.Sleep(500);
            _tcpClient.BeginConnect(_ip, _port, OnTcpConnect, null);
        }

        if(!_tcpClient.Connected)
            return;

        _networkStream = _tcpClient.GetStream();
        _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
    }

    private int bytesToRead = 0;
    private bool waitHeader = true;
    private Queue<byte> currentBytes = new Queue<byte>();

    private bool TryCutHeader(Queue<byte> data, out int header)
    {
        int headerByteSize = sizeof(int);
        if (data.Count < headerByteSize)
        {
            header = -1;
            return false;
        }
        byte[] headerBytes = new byte[headerByteSize];

        for (int i = 0; i < headerByteSize; i++)
        {
            headerBytes[i] = data.Dequeue();
        }
        
        header = BitConverter.ToInt32(headerBytes);
        return true;
    }

    private void ProcessBytes()
    {
        if(currentBytes.Count <= 0)
            return;

        if (waitHeader)
        {
            if(!TryCutHeader(currentBytes, out int header))
                return;
            bytesToRead = header;
            waitHeader = false;
        }
        
        if (!waitHeader && bytesToRead <= currentBytes.Count)
        {
            byte[] data = new byte[bytesToRead];
            for (int i = 0; i < bytesToRead; i++)
            {
                data[i] = currentBytes.Dequeue();
            }

            string message = Encoding.Unicode.GetString(data);
            JsonNode jsonMessage = JsonObject.Parse(message);
            RouteMessageFromServer(jsonMessage);
            waitHeader = true;
            ProcessBytes();
        }
    }

    private void DebugQueuePrint(Queue<byte> queue)
    {
        var array = queue.ToArray();
        var s = Encoding.Unicode.GetString(array);
       // JsonNode json = JsonObject.Parse(s);
        Console.WriteLine(s);
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
            
            byte[] _data = new byte[byteSize];
            Array.Copy(_receiveBuffer, _data, byteSize);
            foreach (var dataByte in _data)
            {
                currentBytes.Enqueue(dataByte);
            }

            DebugQueuePrint(currentBytes);
            ProcessBytes();
           
           _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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

        var utcTime = messageObject["utcTime"].GetValue<DateTime>();
        var message = messageObject["message"].GetValue<string>();
        var username = messageObject["username"].GetValue<string>();
        var messageId = messageObject["messageId"].GetValue<int>();

        OnMessageFromServer(new MessageData()
        {
            Message = message,
            MessageId = messageId,
            Username = username,
            UtcTime = utcTime
        });
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