namespace ChatServer.Database;

public struct UserRegistrationData
{
    public Guid UserId;
    public string Salt;
    public string Hash;
}