using ChatClient;
using JsonMessage.DTO;

namespace Console_ChatClient;

public class ConsoleChatClient : IChatListener
{
    private ChatConnection _chat;
    public ConsoleChatClient(ChatConnection chatConnection)
    {
        _chat = chatConnection;
        _chat.AddListener(this);
    }
    public void OnMessageFromServer(MessageDto message)
    {
        string time = message.UtcTime.ToLocalTime().ToShortTimeString();
        Console.WriteLine($"{message.User.Username}: {message.Message} ({time})");
    }

    public void OnServerConnected()
    {
        Console.WriteLine($"Connected to the server");
    }

    public void OnServerDisconnected()
    {
        Console.WriteLine($"WOW! You are disconnected!");
        _chat.BeginConnect(); 
    }

    public void OnUsersOnServerList(UserDto[] users)
    {
        Console.WriteLine("----SERVER USERS-----");
        foreach (var userDto in users)
        {
            Console.WriteLine($"{userDto.Username} with {userDto.Color} color is {userDto.IsOnline}");
        }
        Console.WriteLine("----------------");
    }

    public void OnOtherClientConnected(string username)
    {
        Console.WriteLine($"NEW BOY JOINED : {username} - WELCOME BUDDY");
    }

    public void OnOtherClientDisconnected(string username)
    {
        Console.WriteLine($"USER LEFT : {username}");
    }

    public void OnUserChangedTextColor(string username, string newTextColor)
    {
        Console.WriteLine($"User {username} has changed color to {newTextColor}");
    }

    public void OnLoginSuccess()
    {
        Console.WriteLine($"LOGIN SUCCESS");
        _chat.RequestUsersOnServerData();
        _chat.RequestLastMessages(10);
    }

    public void OnLoginFail()
    {
        Console.WriteLine($"Wrong name or password");
    }
}