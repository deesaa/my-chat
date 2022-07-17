using JsonMessage.DTO;

namespace ChatClient;

public interface IChatListener
{
    public void OnMessageFromServer(MessageDto message);
    public void OnServerConnected();
    public void OnServerDisconnected();
    public void OnUsersOnServerList(UserDto[] users);
    public void OnOtherClientConnected(string username);
    public void OnOtherClientDisconnected(string username);
    public void OnUserChangedTextColor(string username, string newTextColor);
    public void OnLoginSuccess();
    public void OnLoginFail();
}