using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using Ringer.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public class MessagingService
    {
        #region private members
        public HubConnection _hubConnection;
        #endregion

        #region Initializer
        public async Task Init(string url, string token)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2)
                })// default: wait 0,2,10,30 seconds and try to reconnect
                .Build();

            // Handle Hub connection events
            _hubConnection.Closed += err =>
            {
                Closed?.Invoke(this, new ConnectionEventArgs($"HubConnection.Closed event fired\n{err.Message}\n{DateTime.Now}"));
                return Task.CompletedTask;
            };
            _hubConnection.Reconnecting += err =>
            {
                Reconnecting?.Invoke(this, new ConnectionEventArgs($"HubConnection.Reconnecting event fired\n{err.Message}\n{DateTime.Now}"));
                return Task.CompletedTask;
            };
            _hubConnection.Reconnected += message =>
            {
                Reconnected?.Invoke(this, new ConnectionEventArgs($"HubConnection.Reconnected event fired\n{message}\n{DateTime.Now}"));
                return Task.CompletedTask;
            };

            // Handle Hub messages
            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                MessageReceived?.Invoke(this, new SignalREventArgs(message, user));
            });

            _hubConnection.On<string>("Entered", user =>
            {
                SomeoneEntered?.Invoke(this, new SignalREventArgs($"{user} entered.", user));
                //AddLocalMessage($"{user} entered.", user);
            });

            _hubConnection.On<string>("Left", user =>
            {
                SomeoneLeft?.Invoke(this, new SignalREventArgs($"{user} left.", user));
            });
        }
        #endregion

        #region public properties
        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;
        public bool IsConnecting => _hubConnection.State == HubConnectionState.Connecting;
        public bool IsDisconnected => _hubConnection.State == HubConnectionState.Disconnected;
        public bool IsReconnecting => _hubConnection.State == HubConnectionState.Reconnecting;


        //public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public string ConnectionId => _hubConnection.ConnectionId;
        #endregion

        #region Public Methods
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            try
            {
                Connecting?.Invoke(this, new ConnectionEventArgs($"Try to Connect\n{DateTime.Now}"));

                await _hubConnection.StartAsync();

                if (!IsConnected)
                    throw new InvalidOperationException($"Connection failed.");

                Connected?.Invoke(this, new ConnectionEventArgs($"Connected\n{DateTime.Now}\n{_hubConnection.ConnectionId}"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Connection Failed: " + ex.Message);
                ConnectionFailed?.Invoke(this, new ConnectionEventArgs("Connection Failed: " + ex.Message));
            }
        }

        public async Task ConnectAsync(string room, string user)
        {
            await ConnectAsync();

            if (IsConnected)
                await JoinRoomAsync(room, user);
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                Disconnecting?.Invoke(this, new ConnectionEventArgs($"Try to Disconnect\n{DateTime.Now}\n{_hubConnection.ConnectionId}"));

                await _hubConnection.StopAsync();

                if (IsConnected)
                    throw new InvalidOperationException("Disconnection faild");

                Disconnected?.Invoke(this, new ConnectionEventArgs($"Disconnected\n{DateTime.Now}"));
                Debug.WriteLine($"Disconnection completed\n{DateTime.Now}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnection Failed: " + ex.Message);
                DisconnectionFailed?.Invoke(this, new ConnectionEventArgs("Disconnection Failed: " + ex.Message));
            }
        }

        public async Task DisconnectAsync(string room, string user)
        {
            if (IsConnected)
                await LeaveRoomAsync(room, user);

            await DisconnectAsync();
        }

        public async Task JoinRoomAsync(string room, string user)
        {
            if (!IsConnected)
                await ConnectAsync(room, user);

            await _hubConnection.SendAsync("AddToGroup", room, user);
        }

        public async Task LeaveRoomAsync(string room, string user)
        {
            if (!IsConnected)
                await ConnectAsync(room, user);

            await _hubConnection.SendAsync("RemoveFromGroup", room, user);
        }

        public async Task SendMessageToRoomAsync(string room, string sender, string message)
        {
            if (!IsConnected)
                await ConnectAsync(room, sender);

            await _hubConnection.InvokeAsync("SendMessageGroup", room, sender, message);
        }
        #endregion

        #region Public Events
        public event EventHandler<ConnectionEventArgs> Connecting;
        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> ConnectionFailed;

        public event EventHandler<ConnectionEventArgs> Disconnecting;
        public event EventHandler<ConnectionEventArgs> Disconnected;
        public event EventHandler<ConnectionEventArgs> DisconnectionFailed;

        public event EventHandler<ConnectionEventArgs> Closed;
        public event EventHandler<ConnectionEventArgs> Reconnecting;
        public event EventHandler<ConnectionEventArgs> Reconnected;

        public event EventHandler<SignalREventArgs> SomeoneEntered;
        public event EventHandler<SignalREventArgs> SomeoneLeft;
        public event EventHandler<SignalREventArgs> MessageReceived;
        #endregion
    }

    public class ConnectionEventArgs : IChatEventArgs
    {
        public string Message { get; private set; }

        public ConnectionEventArgs(string message)
        {
            Message = message;
        }
    }
}
