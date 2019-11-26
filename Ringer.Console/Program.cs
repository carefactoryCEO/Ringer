using Ringer.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Ringer.Core.Models;
using Ringer.Core.EventArgs;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        #region members
        //static readonly string hubUrl = "https://ringerhub.azurewebsites.net/hubs/chat";
        static readonly string hubUrl = "http://localhost:5000/hubs/chat";
        //static readonly string tokenUrl = "https://ringerhub.azurewebsites.net/auth/login";
        static readonly string tokenUrl = "http://localhost:5000/auth/login";
        static readonly MessagingService messagingService = new MessagingService();

        static readonly string name = "admin";
        static readonly DateTime birthDate = DateTime.Parse("11-11-11");
        static readonly GenderType gender = GenderType.Female;

        static readonly string Room = "Xamarin";
        static readonly HttpClient client = new HttpClient();
        #endregion

        public static async Task Main(string[] args)
        {
            #region Login

            // get json
            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Name = name,
                BirthDate = birthDate,
                Gender = gender,
            });

            // get Token
            HttpResponseMessage response = await client.PostAsync(
                tokenUrl,
                new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            var token = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Got Token: " + token);

            #endregion

            #region Init and Connect to SignalR

            // Subscribe hubconnection messages
            messagingService.Connected += Service_Connected;
            messagingService.Closed += Service_Closed;
            messagingService.Reconnecting += Service_Reconnecting;
            messagingService.Reconnected += Service_Reconnected;

            messagingService.OnMessageReceived += Service_OnMessageReceived;
            messagingService.OnEntered += Service_OnEntered;
            messagingService.OnLeft += Service_OnLeft;

            // Initialize the messaging service
            messagingService.Init(hubUrl, token);

            // Connect to hub
            await messagingService.ConnectAsync(Room, name);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"OK, {hubUrl} 접속({messagingService.HubConnection.ConnectionId})");
            Console.WriteLine("-----------------------------------");

            #endregion

            #region Choose and join the Constants.Room

            // TODO: 콘솔은 admin이므로 현재 열려 있는 방의 목록을 보여준다.
            // TODO: 방에 입장.

            #endregion

            #region Chat
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
                    await messagingService.LeaveRoomAsync(Room, name);

                    // TODO: 방에서 나와 대기실 -> 방 목록 보여준다.
                    Console.WriteLine("다시 접속할까요?");
                    Console.ReadLine();
                }

                else
                {
                    await messagingService.SendMessageToRoomAsync(Room, name, text);
                }

            } while (keepGoing);
            #endregion
        }

        private static void Service_Connected(object sender, SignalREventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Service_Closed(object sender, SignalREventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Service_Reconnecting(object sender, SignalREventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static async void Service_Reconnected(object sender, SignalREventArgs e)
        {
            await messagingService.JoinRoomAsync(Room, name);

            Console.WriteLine(e.Message);
        }

        private static void Service_OnMessageReceived(object sender, SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.User}: {e.Message}");
        }

        private static void Service_OnEntered(object sender, SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");
        }


        private static void Service_OnLeft(object sender, SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");
        }
    }
}
