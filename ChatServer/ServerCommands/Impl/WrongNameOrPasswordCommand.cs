using System.Text.Json;

namespace ChatServer.ServerCommands;

public class WrongNameOrPasswordCommand : Command
{
    public WrongNameOrPasswordCommand(Guid originId) : base(originId) { }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            wrongNamePass = true
        });
    }
}