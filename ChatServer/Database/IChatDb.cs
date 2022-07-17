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