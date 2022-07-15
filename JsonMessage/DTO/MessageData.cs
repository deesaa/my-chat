using System.Net.Http.Json;
using System.Text.Json.Serialization;

public struct MessageData
{
    public DateTime UtcTime;
    public string Message;
    public ulong MessageId;
    public UserData User;
}