using Ringer.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Ringer.Core.EventArgs;
using Ringer.Core.Data;
using Ringer.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        #region members
        static bool server = false;
        static string hubUrl => server ? "https://ringerhub.azurewebsites.net/hubs/chat" : "http://localhost:5000/hubs/chat";
        static string tokenUrl => server ? "https://ringerhub.azurewebsites.net/auth/login" : "http://localhost:5000/auth/login";
        static string listUrl => server ? "https://ringerhub.azurewebsites.net/auth/list" : "http://localhost:5000/auth/list";
        static readonly MessagingService messagingService = new MessagingService();

        static HttpResponseMessage response;
        static string responseString;
        static readonly string name = "Admin";
        static readonly DateTime birthDate = DateTime.Parse("76-07-21");
        static readonly GenderType gender = GenderType.Male;

        // 신모범 : d947c08f-ca2a-44be-b76c-219151c9fb73
        // 김순용 : 7e81ec2e-9fd4-4971-a103-6b10a7d0cbf7
        static string currentRoomId;

        static Dictionary<string, string> roomList = new Dictionary<string, string>();
        static readonly HttpClient client = new HttpClient();
        #endregion

        public static async Task Main(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
                if (args[i] == "server" || args[i] == "azure")
                    server = true;

            #region Login

            // get json
            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Name = name,
                BirthDate = birthDate,
                Gender = gender,
                DeviceType = DeviceType.Console,
                DeviceId = "87989c4a-0bf7-42e3-89dd-abc17bacd2bd"
            });

            Console.WriteLine(tokenUrl);

            // get Token
            response = await client.PostAsync(
                tokenUrl,
                new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ResponseJson>(responseString);

            var token = responseObject.token;

            Console.WriteLine($"Got Token: " + token);

            #endregion

            #region Init and Connect to SignalR

            // Subscribe messagingService events
            messagingService.Connecting += (s, e) => Console.WriteLine(e.Message);
            messagingService.ConnectionFailed += (s, e) => Console.WriteLine(e.Message);
            messagingService.Connected += (s, e) => Console.WriteLine(e.Message);

            messagingService.Closed += (s, e) => Console.WriteLine(e.Message);
            messagingService.Reconnecting += (s, e) => Console.WriteLine(e.Message);
            messagingService.Reconnected += async (s, e) =>
            {
                Console.WriteLine(e.Message);
                if (currentRoomId != null)
                    await messagingService.JoinRoomAsync(currentRoomId, name);
            };

            messagingService.MessageReceived += Service_OnMessageReceived;
            messagingService.SomeoneEntered += Service_OnEntered;
            messagingService.SomeoneLeft += Service_OnLeft;

            // Initialize the messaging service
            messagingService.Init(hubUrl, token);

            // Connect to hub
            await messagingService.ConnectAsync();

            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"OK, {hubUrl} 접속({messagingService.ConnectionId})");
            Console.WriteLine("-----------------------------------");
            #endregion

            #region room list

            await RefreshUserListAsync();

            await ShowUsersAsync();
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
                    keepGoing = false;

                else if (roomList.ContainsKey(text))
                {
                    await messagingService.JoinRoomAsync(roomList[text], name);

                    Console.WriteLine("--- Joined to " + text + " ---");

                    currentRoomId = roomList[text];
                }

                else if (text == "leave")
                {
                    await messagingService.LeaveRoomAsync(currentRoomId, name);
                    currentRoomId = null;
                    await ShowUsersAsync();
                }

                else if (text == "list")
                    await ShowUsersAsync();

                else
                {
                    if (currentRoomId != null)
                        await messagingService.SendMessageToRoomAsync(currentRoomId, name, text);
                    else
                        await ShowUsersAsync();

                }

            } while (keepGoing);
            #endregion
        }

        private static async Task RefreshUserListAsync()
        {
            response = await client.GetAsync(listUrl);
            responseString = await response.Content.ReadAsStringAsync();
            List<Room> rooms = JsonSerializer.Deserialize<List<Room>>(responseString);

            foreach (var room in rooms)
            {
                if (!roomList.ContainsKey(room.Name))
                    roomList.Add(room.Name, room.Id);
            }
        }

        private static async Task ShowUsersAsync()
        {
            await RefreshUserListAsync();

            Console.WriteLine("대화방을 선택하세요.");
            Console.WriteLine("----------- room list -----------");
            foreach (var room in roomList)
            {
                Console.WriteLine($"{room.Key}");
            }
            Console.WriteLine("---------------------------------");
        }

        private static void Service_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {

            if (e.SenderName == name)
                return;

            Console.WriteLine($"{e.SenderName}: {e.Body}");

        }

        private static void Service_OnEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender == name)
                return;

            Console.WriteLine($"{e.Message}");
        }


        private static void Service_OnLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender == name)
                return;

            Console.WriteLine($"{e.Message}");
        }
    }
}
