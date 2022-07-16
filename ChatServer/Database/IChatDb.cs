using System.Collections.ObjectModel;

namespace ChatServer.Database;

public interface IChatDb
{
    public UserData? GetUserData(Guid userId);
    public bool AddUserData(UserData userData);
    void UpdateUserData(UserData user);
    public ReadOnlyCollection<UserData> GetAllUsers();
    public ReadOnlyCollection<MessageData> GetAllMessages();
    void AddMessage(MessageData message);
}

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