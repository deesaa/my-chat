using ChatServer.Database;

namespace ChatServer.ServerCommands;

public abstract class Command
{
    protected readonly DateTime UtcTime;
    public Guid OriginId { get; private set; }
    
    public Command(Guid originId)
    {
        OriginId = originId;
        UtcTime = DateTime.UtcNow;
    }

    protected Command(Guid originId, DateTime time)
    {
        OriginId = originId;
        UtcTime = time;
    }

    public abstract string ToJson();
}