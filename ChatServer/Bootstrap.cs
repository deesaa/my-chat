// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ChatServer;
using ChatServer;

public static class Bootstrap
{
    private static Server _server;
    static void Main(string[] args)
    {
        Console.Title = "Chat Server";
        _server = new Server(26950);
        Console.WriteLine("Press any key to shut down the server");
        Console.ReadKey();
    }
}