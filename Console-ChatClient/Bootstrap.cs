public static class Bootstrap
{
    static void Main(string[] args)
    {
        ChatConnection chat = new ChatConnection();

        chat.OnMessageFromServer = messageObject =>
        {
            string time = messageObject.UtcTime.ToLocalTime().ToShortTimeString();
            Console.WriteLine($"{messageObject.Username}: {messageObject.Message} ({time})");
        };

        chat.OnOtherClientConnected = name => Console.WriteLine($"NEW BOY JOINED : {name} - WELCOME BUDDY");

        chat.OnOtherClientDisconnected = name => Console.WriteLine($"USER LEFT : {name}");

        chat.OnServerDisconnected = () =>
        {
            Console.WriteLine($"WOW! You are disconnected!");
            ConnectionLoop(chat);
        };
        
        chat.OnServerConnected = () => Console.WriteLine($"Connected to the server");
        
        chat.OnUserNameColorChanged = () => Console.WriteLine($"Connected to the server");

        chat.SetMessageValidator(ValidatorFactory.GetDefaultMessageValidator());
        chat.SetNameValidator(ValidatorFactory.GetDefaultNameValidator());
        chat.SetMessageSterilizer(new TrimSterilizer());
        chat.SetNameSterilizer(new EmptyCharsSterilizer());

        ConnectionLoop(chat);
        
        while (true)
        {
            string message = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack();
            chat.Write(message);
        }
    }

    private static void ConnectionLoop(ChatConnection chat)
    {
        chat.BeginConnect();
        while (!chat.Connected || !chat.IsNameSet)
        {
            Console.WriteLine("Enter Name:");
            string name = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack();
            chat.SetName(name);
        }
        chat.RequestMessageHistory(10);
    }
}