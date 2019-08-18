using Ringer.Core;
using System;
using System.Threading.Tasks;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        static ChatService service;
        static string room = "Xamarin";
        static string name;
        static Random random = new Random();

        public static async Task Main(string[] args)
        {
            name = "Ringer" + random.Next(1,100);

            service = new ChatService();
            service.OnReceivedMessage += Service_OnReceivedMessage;
            service.OnConnectionClosed += Service_OnConnectionClosed;
            service.OnEnteredOrExited += Service_OnEnteredOrExited;

            service.Init("ringerchat.azurewebsites.net", true);

            await service.ConnectAsync();
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("       OK, 링거 호스트 접속");

            await JoinRoom();
            

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

                    Console.WriteLine("다시 접속할까요?");
                    Console.ReadLine();

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
            await service.JoinChannelAsync(room, name);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("         링거 서비스 채팅             ");
            Console.WriteLine("-----------------------------------");
        }

        private static void Service_OnReceivedMessage(object sender, Core.EventArgs.ChatEventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.User}: {e.Message}");
        }
    }
}
