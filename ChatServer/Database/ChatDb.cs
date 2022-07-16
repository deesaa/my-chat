using System.Collections.ObjectModel;

namespace ChatServer.Database;

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
    
    public void UpdateUserData(UserData userData)
    {
        if (!_usersData.ContainsKey(userData.Id))
            throw new Exception($"Trying update not existing user {userData.Username}");
        _usersData[userData.Id] = userData;
    }

    public ReadOnlyCollection<UserData> GetAllUsers() => _usersData.Values.ToList().AsReadOnly();
    public ReadOnlyCollection<MessageData> GetAllMessages() => _messages.AsReadOnly();
    public void AddMessage(MessageData message)
    {
        _messages.Add(message);
    }
}