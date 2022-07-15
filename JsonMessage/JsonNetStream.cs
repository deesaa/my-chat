using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using JsonMessage;

namespace ChatServer;

public class JsonNetStream
{
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private MessageDecoder _messageDecoder;
    
    public Action<JsonObject> OnNext;
    public Action OnDisconnect;

    private byte[] _receiveBuffer;
    private int _bufferSize;
    public JsonNetStream(TcpClient tcpClient)
    {
        _bufferSize = tcpClient.ReceiveBufferSize;
        _receiveBuffer = new byte[_bufferSize];
        _tcpClient = tcpClient;
        _messageDecoder = new MessageDecoder();
        _messageDecoder.OnNext = message =>
        {
            var jsonObject = JsonObject.Parse(message).AsObject();
            OnNext(jsonObject);
        };
    }

    public void Close()
    {
        _networkStream.Close();
    }

    public void BeginRead()
    {
        _networkStream = _tcpClient.GetStream();
        _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);
    }
    
    private void OnNetworkStreamData(IAsyncResult receive)
    {
        try
        {
            int byteSize = _networkStream.EndRead(receive);
            if (byteSize <= 0)
            {
                OnDisconnect();
                return;
            }
            
            byte[] receivedBytes = new byte[byteSize];
            Array.Copy(_receiveBuffer, receivedBytes, byteSize);
            _messageDecoder.PutBytes(receivedBytes);
            _networkStream.BeginRead(_receiveBuffer, 0, _bufferSize, OnNetworkStreamData, null);

        }
        catch (Exception e)
        { 
            Console.WriteLine("OnNetworkStreamData Exception: " + e);
        }
    }

    public void Write(string jsonMessage)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(jsonMessage);
        int byteSize = bytes.Length;
        _networkStream.Write(BitConverter.GetBytes(byteSize));
        _networkStream.Write(bytes);
    }
}