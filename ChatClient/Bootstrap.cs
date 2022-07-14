public static class Bootstrap
{
    static void Main(string[] args)
    {
        ChatConnection chat = new ChatConnection();

        chat.OnMessageFromServer = messageObject =>
        {
            Console.WriteLine($"{messageObject.Username}: {messageObject.Message} ({messageObject.UtcTime.ToLocalTime().ToShortTimeString()})");
        };

        chat.OnOtherClientConnected = name =>
        {
            Console.WriteLine($"NEW BOY JOINED : {name} - WELCOME BUDDY");
        };
        
        chat.OnOtherClientDisconnected = name =>
        {
            Console.WriteLine($"USER LEFT : {name}");
        };

        chat.SetMessageValidator(ValidatorFactory.GetDefaultMessageValidator());
        chat.SetNameValidator(ValidatorFactory.GetDefaultNameValidator());
        chat.SetMessageSterilizer(new TrimSterilizer());
        chat.SetNameSterilizer(new EmptyCharsSterilizer());

        while (!chat.IsNameSet)
        {
            Console.WriteLine("Enter Name:");
            string name = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack();
            chat.SetName(name);
        }
        
        chat.RequestMessageHistory(10);

        while (true)
        {
            // Console.WriteLine("Enter Message:");
            string message = Console.ReadLine() ?? " ";
            Ext.ConsoleLineBack();
            chat.Write(message);
        }
    }
}