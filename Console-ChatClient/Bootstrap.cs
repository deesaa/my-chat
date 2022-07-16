using ChatClient;
using ChatClient.Configuration;

namespace Console_ChatClient;

public static class Bootstrap
{
    static void Main(string[] args)
    {
        ChatConfiguration chatConfiguration = ChatConfigurationFactory.GetDefaultConfiguration();
        ChatConnection chat = new ChatConnection(chatConfiguration);

        chat.OnMessageFromServer = messageObject =>
        {
            string time = messageObject.UtcTime.ToLocalTime().ToShortTimeString();
            Console.WriteLine($"{messageObject.User.Username}: {messageObject.Message} ({time})");
        };

        chat.OnOtherClientConnected = name => Console.WriteLine($"NEW BOY JOINED : {name} - WELCOME BUDDY");

        chat.OnOtherClientDisconnected = name => Console.WriteLine($"USER LEFT : {name}");

        chat.OnServerDisconnected = () =>
        {
            Console.WriteLine($"WOW! You are disconnected!");
            chat.BeginConnect(); 
        };
        
        chat.OnServerConnected = () =>
        {
            Console.WriteLine($"Connected to the server");
        };
        
        chat.OnLoginSuccess = () =>
        {        
            Console.WriteLine($"LOGIN SUCCESS");
            _tryingAuth = false;
            chat.RequestUsersOnServerData();
            chat.RequestLastMessages(10);
        };

        chat.OnLoginFail = () =>
        {
            _tryingAuth = false;
            Console.WriteLine($"Wrong name or password");
        };
        
        chat.OnUserNameColorChanged = (name, newColor) =>
        {
            Console.WriteLine($"User {name} has changed color to {newColor}");
        };

        chat.OnUsersOnServerList = (usersList) =>
        {
            Console.WriteLine("----SERVER USERS-----");
            foreach (var userDto in usersList)
            {
                Console.WriteLine($"{userDto.Username} with {userDto.Color} color is {userDto.IsOnline}");
            }
            Console.WriteLine("----------------");
        };
        
        chat.BeginConnect();
        
        
        
        while (true)
        {
            if (_tryingAuth)
            {
                Thread.Yield();
                continue;
            }
            
            if (!chat.IsAuthorized)
            {
                bool isEnterDataValid = TryWriteLogin(chat);
                if(isEnterDataValid)
                    _tryingAuth = true;
            }
            else
            {
                WriteMessage(chat);
            }
        }
        //TODO: AUTORECONNECTION IS BROKEN 
    }

    private static bool _tryingAuth = false;
    
    
    
    private static bool TryWriteLogin(ChatConnection chat)
    {
        Console.WriteLine("Enter Name:");
        string name = Console.ReadLine() ?? " ";
        Console.WriteLine("Enter Password:");
        string password = Console.ReadLine() ?? " ";
        Ext.ConsoleLineBack();
        return chat.TryEnterNamePass(name, password);
    }

    private static void WriteMessage(ChatConnection chat)
    {
        string message = Console.ReadLine() ?? " ";
        Ext.ConsoleLineBack();
        chat.Write(message);
    }
}