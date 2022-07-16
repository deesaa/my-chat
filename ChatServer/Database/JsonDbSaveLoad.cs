using System.Text;
using Newtonsoft.Json;

namespace ChatServer.Database;

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