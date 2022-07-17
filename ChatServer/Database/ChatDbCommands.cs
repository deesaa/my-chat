namespace ChatServer.Database;

public static class ChatDbCommands
{
    public static void SetUserOnline(this IChatDb db, UserData user, bool isOnline)
    {
        user = new UserData()
        {
            Color = user.Color,
            Id = user.Id,
            Password = user.Password,
            Username = user.Username,
            IsOnline = isOnline
        };
        db.UpdateUserData(user);
    }
    
    public static void SetUserColor(this IChatDb db, UserData user, string color)
    {
        user = new UserData()
        {
            Color = color,
            Id = user.Id,
            Password = user.Password,
            Username = user.Username,
            IsOnline = user.IsOnline
        };
        db.UpdateUserData(user);
    }
}