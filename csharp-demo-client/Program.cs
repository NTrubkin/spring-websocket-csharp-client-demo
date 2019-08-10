using System;
using System.Collections.Generic;
using NikitaTrubkin.StompClient;

namespace csharp_demo_client
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            IStompClient client = new StompWebsocketClient("ws://localhost:8080/ws/websocket");
            client.Connect(new Dictionary<string, string>());
            client.Subscribe<Greeting>("/topic/greetings", new Dictionary<string, string>(), SayHello);
            client.Send(new HelloMessage(){Name = "Nikita"}, "/app/hello", new Dictionary<string, string>());
            Console.ReadKey(true);
        }

        private static void SayHello(object sender, Greeting body)
        {
            Console.WriteLine($"Server says: {body.Content}");
        }
    }
}