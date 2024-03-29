﻿using ChatClient;
using ChatClient.Configuration;

namespace Console_ChatClient;

public static class Bootstrap
{
    static void Main(string[] args)
    {
        ChatConfiguration chatConfiguration = ChatConfigurationFactory.GetDefaultConfiguration();
        ChatConnection chat = new ChatConnection(chatConfiguration);
        ConsoleChatClient consoleChatListener = new ConsoleChatClient(chat);
        chat.BeginConnect();
        
        while (true)
        {
            if (chat.TryingAuth)
            {
                Thread.Yield();
                continue;
            }
            
            if (!chat.IsAuthorized)
            {
                bool isEnterDataValid = TryWriteLogin(chat);
                if(isEnterDataValid)
                    chat.TryingAuth = true;
            }
            else
            {
                WriteMessage(chat);
            }
        }
    }

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

        if (message.Contains("setColor:"))
        {
            string color = message.Replace("setColor:", "");
            chat.ChangeTextColor(color);
            return;
        }
        chat.WriteMessage(message);
    }
}