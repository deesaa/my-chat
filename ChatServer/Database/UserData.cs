namespace ChatServer.Database;

public struct UserData
{
    public Guid Id;
    public string Username;
    public string Color;
    public string Password;
    public bool IsOnline;

    public UserData(Guid clientId, string name, string password)
    {
        Id = clientId;
        Username = name;
        Color = "Default";
        Password = password;
        IsOnline = true;
    }

    public void SetOffline()
    {
        IsOnline = false;
    }
}