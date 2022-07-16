using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace JsonMessage.DTO;

[Serializable]
public struct MessageDto
{
    public DateTime UtcTime;
    public string Message;
    public UserDto User;
}