using ChatServer.Database;
using JsonMessage.DTO;
using Newtonsoft.Json;

namespace ChatServer.ServerCommands;

public class MessageCommand : Command
{
    private MessageDto _messageDto;
    public MessageDto MessageDto => _messageDto;
    
    public MessageCommand(UserData user, string message) : base(user.Id)
    {
        _messageDto = new MessageDto()
        {
            Message = message,
            UtcTime = this.UtcTime,
            User = new UserDto()
            {
                Color = user.Color,
                Id = user.Id,
                Username = user.Username
            }
        };
    }

    public MessageCommand(MessageData messageData, UserData user) 
        : base(messageData.SenderUserId, messageData.UtcSendDate)
    {
        _messageDto = new MessageDto()
        {
            Message = messageData.MessageText,
            UtcTime = UtcTime,
            User = new UserDto()
            {
                Color = user.Color,
                Id = this.OriginId,
                Username = user.Username
            }
        };
    }

    public override string ToJson()
    {
        string json = JsonConvert.SerializeObject(new
        {
            userMessage = _messageDto
        });
        return json;
    }

    public MessageData CreateData()
    {
        return new MessageData()
        {
            MessageText = _messageDto.Message,
            SenderUserId = _messageDto.User.Id,
            UtcSendDate = _messageDto.UtcTime
        };
    }
}