using System.Text;

namespace JsonMessage;

public class MessageStream
{
    private Queue<byte> _dataQueue = new();
    private bool _needHeader = true;
    private int _nextMessageByteSize = -1;
    
    public Action<string> OnNext;
    
    private void WriteMessage(byte[] bytes)
    {
        return;
        int messageByteCount = bytes.Length;
        byte[] messageByteCountBytes = BitConverter.GetBytes(messageByteCount);
        PutBytes(messageByteCountBytes);
        PutBytes(bytes);
        ProcessQueue();
    }
    
    public void PutBytes(byte[] bytes)
    {
        foreach (var @byte in bytes)
        {
            _dataQueue.Enqueue(@byte);
        }
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if(_dataQueue.Count <= 0)
            return;

        if (_needHeader)
        {
            if(!TryGetHeader(out int header))
                return;
            _nextMessageByteSize = header;
            _needHeader = false;
        }
        
        if (!_needHeader && _nextMessageByteSize <= _dataQueue.Count)
        {
            byte[] data = new byte[_nextMessageByteSize];
            for (int i = 0; i < _nextMessageByteSize; i++)
            {
                data[i] = _dataQueue.Dequeue();
            }

            string message = Encoding.Unicode.GetString(data);
            OnNext(message);
            _needHeader = true;
            ProcessQueue();
        }
    }
    
    private bool TryGetHeader(out int nextMessageByteSize)
    {
        int headerByteSize = sizeof(int);
        if (_dataQueue.Count < headerByteSize)
        {
            nextMessageByteSize = -1;
            return false;
        }
        byte[] headerBytes = new byte[headerByteSize];

        for (int i = 0; i < headerByteSize; i++)
        {
            headerBytes[i] = _dataQueue.Dequeue();
        }
        
        nextMessageByteSize = BitConverter.ToInt32(headerBytes);
        return true;
    }
}