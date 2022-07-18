using System.Text.Json;

namespace ChatServer.ServerCommands;

public class UserChangedTextColorCommand : Command
{
    private string _userName;
    private string _newColor;

    public UserChangedTextColorCommand(Guid originId, string userName, string newColor) : base(originId)
    {
        _userName = userName;
        _newColor = newColor;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userChangedTextColor = _newColor,
            userName = _userName
        });
    }
}