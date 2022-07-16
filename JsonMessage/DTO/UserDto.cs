namespace JsonMessage.DTO;

[Serializable]
public struct UserDto
{
    public Guid Id;
    public string Username;
    public string Color;
    public bool IsOnline;
}