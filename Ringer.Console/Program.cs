using Ringer.Core;
using System;
using System.Threading.Tasks;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        static ChatService service;
        static string room;
        static string name;
        static Random random = new Random();

        public static async Task Main(string[] args)
        {
            name = "console";

            service = new ChatService();
            service.OnReceivedMessage += Service_OnReceivedMessage;
            service.OnConnectionClosed += Service_OnConnectionClosed;
            service.OnEnteredOrExited += Service_OnEnteredOrExited;

            Console.WriteLine("Now you get the ringer chat rooms...");

            var ip = "ringerchat.azurewebsites.net";
            service.Init(ip, ip != "localhost");

            await service.ConnectAsync();
            Console.WriteLine("OK, you are connected");

            await JoinRoom();
            Console.WriteLine("You can now chat with your friends.");

            var keepGoing = true;
            do
            {
                var text = Console.ReadLine();

                if (text == "exit")
                {
                    keepGoing = false;
                }

                else if (text == "leave")
                {
                    await service.LeaveChannelAsync(room, name);
                    await JoinRoom();
                }

                else
                {
                    await service.SendMessageAsync(room, name, text);
                }

            } while (keepGoing);

        }

        private static void Service_OnEnteredOrExited(object sender, Core.EventArgs.ChatEventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");

        }

        private static void Service_OnConnectionClosed(object sender, Core.EventArgs.ChatEventArgs e)
        {
            Console.WriteLine($"Disconnected at {DateTime.Now}");
        }

        private static async Task JoinRoom()
        {
            Console.WriteLine($"Enter room ({string.Join(",", service.GetRooms())}):");
            room = Console.ReadLine();

            await service.JoinChannelAsync(room, name);
        }

        private static void Service_OnReceivedMessage(object sender, Core.EventArgs.ChatEventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.User}: {e.Message}");
        }
    }
}
