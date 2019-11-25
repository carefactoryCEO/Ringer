using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using Ringer.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public class MessagingService
    {
        private static bool IsInitialized = false;

        #region Initializer
        public void Init(string url, string token)
        {
            if (IsInitialized)
                return;

            HubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect() // wait 0,2,10,30 seconds and try to reconnect
                .Build();

            // Handle Hub connection events
            HubConnection.Closed += err =>
            {
                Closed?.Invoke(this, new SignalREventArgs($"HubConnection.Closed event fired\n{err.Message}\n{DateTime.Now}", "system"));
                AddLocalMessage($"HubConnection.Closed event fired\n{err.Message}\n{DateTime.Now}", "system");
                return Task.CompletedTask;
            };
            HubConnection.Reconnecting += err =>
            {
                Reconnecting?.Invoke(this, new SignalREventArgs($"HubConnection.Reconnecting event fired\n{err.Message}\n{DateTime.Now}", "system"));
                AddLocalMessage($"HubConnection.Reconnecting event fired\n{err.Message}\n{DateTime.Now}", "system");
                return Task.CompletedTask;
            };
            HubConnection.Reconnected += message =>
            {
                Reconnected?.Invoke(this, new SignalREventArgs($"HubConnection.Reconnected event fired\n{message}\n{DateTime.Now}", "system"));
                AddLocalMessage($"HubConnection.Reconnected event fired\n{message}\n{DateTime.Now}", "system");
                return Task.CompletedTask;
            };

            // Handle Hub messages
            HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(this, new SignalREventArgs(message, user));
                AddLocalMessage(message, user);
            });
            HubConnection.On<string>("Entered", user =>
            {
                OnEntered?.Invoke(this, new SignalREventArgs($"{user} entered.", user));
                //AddLocalMessage($"{user} entered.", user);
            });
            HubConnection.On<string>("Left", user =>
            {
                OnLeft?.Invoke(this, new SignalREventArgs($"{user} left.", user));
                AddLocalMessage($"{user} left.", user);
            });

            IsInitialized = true;
        }
        #endregion

        #region public properties
        public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
        public bool IsConnecting => HubConnection.State == HubConnectionState.Connecting;
        public bool IsDisconnected => HubConnection.State == HubConnectionState.Disconnected;
        public bool IsReconnecting => HubConnection.State == HubConnectionState.Reconnecting;

        public HubConnection HubConnection { get; private set; }
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        #endregion

        #region Public Methods
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            try
            {
                await HubConnection.StartAsync();

                if (!IsConnected)
                    throw new InvalidOperationException("SignalRService.ConnectAsync() faild");

                Connected?.Invoke(this, new SignalREventArgs($"Connected. ConnectionID: {HubConnection.ConnectionId}", "system"));

                // TODO: 방 입장 로직 변경
                //await JoinRoomAsync("Xamarin", "me");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Connection Failed: " + ex.Message);
                AddLocalMessage("Connection Failed: " + ex.Message, "system");
            }
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                Debug.WriteLine($"Try to Disconnect\n{DateTime.Now}\n{HubConnection.ConnectionId}");
                AddLocalMessage($"Try to Disconnect\n{DateTime.Now}\n{HubConnection.ConnectionId}", "system");

                await HubConnection.StopAsync();

                if (IsConnected)
                    throw new InvalidOperationException("SignalRService.DisconnectAsync() faild");

                Debug.WriteLine($"Disconnection completed\n{DateTime.Now}");
                AddLocalMessage($"Disconnection completed\n{DateTime.Now}", "system");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnection Failed: " + ex.Message);
                AddLocalMessage("Disconnection Failed: " + ex.Message, "system");
            }
        }

        public async Task JoinRoomAsync(string room, string user)
        {
            if (IsConnected)
                await HubConnection.SendAsync("AddToGroup", room, user);
        }

        public async Task LeaveRoomAsync(string group, string user)
        {
            if (IsConnected)
                await HubConnection.SendAsync("RemoveFromGroup", group, user);
        }

        public async Task SendMessageToRoomAsync(string group, string sender, string message)
        {
            if (!IsConnected)
                await ConnectAsync();

            await HubConnection.InvokeAsync("SendMessageGroup", group, sender, message);
        }

        public void AddLocalMessage(string message, string user)
        {
            Messages.Insert(0, new Message
            {
                Content = $"{user}: {message}",
                Sender = user
            });
        }
        #endregion

        #region Public Events
        public event EventHandler<SignalREventArgs> Connected;
        public event EventHandler<SignalREventArgs> Closed;
        public event EventHandler<SignalREventArgs> Reconnecting;
        public event EventHandler<SignalREventArgs> Reconnected;

        public event EventHandler<SignalREventArgs> OnEntered;
        public event EventHandler<SignalREventArgs> OnLeft;
        public event EventHandler<SignalREventArgs> OnMessageReceived;
        #endregion
    }
}