using System.Collections.ObjectModel;

namespace ChatServer.Database;

public struct UserRegistrationData
{
    public Guid UserId;
    public string Salt;
    public string Hash;
}

public struct MessageData
{
    public Guid SenderUserId;
    public string MessageText;
    public DateTime UtcSendDate;
}

public interface IChatDb
{
    public UserData? GetUserData(Guid userId);
    public bool AddUserData(UserData userData);
    public ReadOnlyCollection<UserData> GetAllUsers();
    public ReadOnlyCollection<MessageData> GetAllMessages();
}

public class ChatDb : IChatDb
{
    private Dictionary<Guid, UserData> _usersData = new();
    private Dictionary<Guid, UserRegistrationData> _usersRegistrationData = new();
    private List<MessageData> _messages = new();
    
    
    public UserData? GetUserData(Guid userId) => _usersData.ContainsKey(userId) ? _usersData[userId] : null;
    public bool AddUserData(UserData userData)
    {
        if (_usersData.ContainsKey(userData.Id))
            return false;
        _usersData[userData.Id] = userData;
        return true;
    }

    public ReadOnlyCollection<UserData> GetAllUsers() => _usersData.Values.ToList().AsReadOnly();
    public ReadOnlyCollection<MessageData> GetAllMessages() => _messages.AsReadOnly();

    public static IChatDb CreateOrLoad()
    {
        
    }
}