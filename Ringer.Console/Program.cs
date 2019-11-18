using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core;
using System;
using System.Threading.Tasks;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        static SignalRService service;
        static string room = "Xamarin";
        static string name;
        static Random random = new Random();

        public static async Task Main(string[] args)
        {
            name = "Ringer" + random.Next(1,100);

            service = new SignalRService();
            service.OnReceivedMessage += Service_OnReceivedMessage;
            service.Closed += Service_OnConnectionClosed;
            service.OnEntered += Service_OnEntered;

            //Console.WriteLine("url?");
            //string url = Console.ReadLine();
            //service.Init(url, true);


            //service.Init("ringerchat.azurewebsites.net", true);
            service.Init("localhost:44389", name, room);

            await service.ConnectAsync();
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"       OK, 링거 호스트 접속({service.HubConnection.ConnectionId})");

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

                else if (text == "test")
                {
                    var result = await service.HubConnection.InvokeAsync<string>("ReadyToDisconnect", name);

                    Console.WriteLine(result);
                }

                else
                {
                    await service.SendMessageToGroupAsync(room, name, text);
                }

            } while (keepGoing);

        }

        private static void Service_OnEntered(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");

        }

        private static void Service_On(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");

        }

        private static void Service_OnConnectionClosed(object sender, Core.EventArgs.SignalREventArgs e)
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

        private static void Service_OnReceivedMessage(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.User}: {e.Message}");
        }
    }
}
