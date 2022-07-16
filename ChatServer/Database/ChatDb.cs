using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;

namespace ChatServer.Database;


public interface ISaveLoad
{
    public void Save();
    public void Load();

}

public class JsonDbSaveLoad : ISaveLoad
{
    private IChatDb _db;
    private string _saveFileName;
    public JsonDbSaveLoad(IChatDb db, string saveFileName)
    {
        _saveFileName = saveFileName;
        _db = db;
    }
    public void Save()
    {
        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Join(directory, _saveFileName);
        
        using (var fileStream = File.Create(filePath))
        {
            var json = JsonConvert.SerializeObject(new DbSaveObject()
            {
                Users = _db.GetAllUsers().ToList(),
                Messages = _db.GetAllMessages().ToList()
            });

            using (var writer = new StreamWriter(fileStream, Encoding.Unicode))
            {
                writer.WriteAsync(json);
            }
        }
    }
    
    [Serializable]
    public class DbSaveObject
    {
        public List<UserData> Users;
        public List<MessageData> Messages;
    }

    public void Load()
    {
        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Join(directory, _saveFileName);
        
        if(!File.Exists(filePath))
            return;
        
        using (var fileStream = File.Open(filePath, FileMode.Open))
        {
            using (var reader = new StreamReader(fileStream, Encoding.Unicode))
            {
                string json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<DbSaveObject>(json);
                
                foreach (var user in data.Users)
                {
                    user.SetOffline();
                    _db.AddUserData(user);
                }
                
                foreach (var message in data.Messages)
                {
                    _db.AddMessage(message);
                }
            }
        }
    }
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