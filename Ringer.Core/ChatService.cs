using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public class ChatService
    {
        #region evet handlers
        public event EventHandler<ChatEventArgs> OnReceivedMessage;
        public event EventHandler<ChatEventArgs> OnEnteredOrExited;
        public event EventHandler<ChatEventArgs> OnConnectionClosed;
        public event EventHandler<ChatEventArgs> OnReconnected;
        #endregion

        #region private members
        HubConnection hubConnection;
        Random random;
        #endregion

        #region public properties
        public bool IsConnected { get; set; }
        public Dictionary<string, string> ActiveChannels { get; } = new Dictionary<string, string>();
        #endregion

        #region constructor and destructor
        ~ChatService()
        {
            Console.WriteLine("Destructor was called");
            OnConnectionClosed?.Invoke(this, new ChatEventArgs($"ChatService.Destructor:Service Destructed..{DateTime.Now}", string.Empty));
        }
        #endregion

        #region public methods
        public void Init(string urlRoot, bool useHttps)
        {
            random = new Random();

            var port = (urlRoot == "localhost" || urlRoot == "10.0.2.2") ?
                (useHttps ? ":5001" : ":5000") :
                string.Empty;

            var url = $"http{(useHttps ? "s" : string.Empty)}://{urlRoot}{port}/hubs/chat";
            hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();

            hubConnection.Closed += async (error) =>
            {
                OnConnectionClosed?.Invoke(this, new ChatEventArgs($"ChatService.HubConnection.Closed:Connection closed...{DateTime.Now}", string.Empty));
                ActiveChannels.Clear();
                IsConnected = false;
                await Task.Delay(random.Next(0, 5) * 1000);
                try
                {
                    await ConnectAsync();
                    OnReconnected?.Invoke(this, new ChatEventArgs($"ChatService.HubConnection.Closed:Reconnected...{DateTime.Now}", string.Empty));
                }
                catch (Exception ex)
                {
                    // Exception!
                    Debug.WriteLine(ex);
                }
            };

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnReceivedMessage?.Invoke(this, new ChatEventArgs(message, user));
            });

            hubConnection.On<string>("Entered", (user) =>
            {
                OnEnteredOrExited?.Invoke(this, new ChatEventArgs($"{user} entered.", user));
            });


            hubConnection.On<string>("Left", (user) =>
            {
                OnEnteredOrExited?.Invoke(this, new ChatEventArgs($"{user} left.", user));
            });


            hubConnection.On<string>("EnteredOrLeft", (message) =>
            {
                OnEnteredOrExited?.Invoke(this, new ChatEventArgs(message, message));
            });
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            await hubConnection.StartAsync();
            IsConnected = true;
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;

            try
            {
                await hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            ActiveChannels.Clear();
            IsConnected = false;
        }

        public async Task LeaveChannelAsync(string group, string userName)
        {
            if (!IsConnected || !ActiveChannels.ContainsKey(group))
                return;

            await hubConnection.SendAsync("RemoveFromGroup", group, userName);

            ActiveChannels.Remove(group);
        }

        public async Task JoinChannelAsync(string group, string userName)
        {
            if (!IsConnected || ActiveChannels.ContainsKey(group))
                return;

            await hubConnection.SendAsync("AddToGroup", group, userName);
            ActiveChannels.Add(group, userName);

        }

        public async Task SendMessageAsync(string group, string userName, string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            await hubConnection.InvokeAsync("SendMessageGroup",
                    group,
                    userName,
                    message);
        }

        public List<string> GetRooms()
        {
            return new List<string>
            {
                "ringer",
                "carefactory",
                "Xamarin"
            };
        }
        #endregion
    }
}