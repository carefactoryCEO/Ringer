using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public interface IMessagingService
    {
        bool IsConnected { get; } // stable
        bool IsDisconnected { get; } // connect immediatly
        bool IsConnecting { get; } // indicate activity
        bool IsReconnecting { get; } // indicate activity
        string ConnectionId { get; }

        void Init(string url, string token);

        Task ConnectAsync();
        Task DisconnectAsync();
        Task JoinRoomAsync(string room, string user);
        Task LeaveRoomAsync(string room, string user);
        Task<int> SendMessageToRoomAsync(string roomId, string sender, string body);

        event EventHandler<ConnectionEventArgs> Connecting;
        event EventHandler<ConnectionEventArgs> Connected;
        event EventHandler<ConnectionEventArgs> ConnectionFailed;
        event EventHandler<ConnectionEventArgs> Disconnecting;
        event EventHandler<ConnectionEventArgs> Disconnected;
        event EventHandler<ConnectionEventArgs> DisconnectionFailed;
        event EventHandler<ConnectionEventArgs> Reconnecting;
        event EventHandler<ConnectionEventArgs> Reconnected;
        event EventHandler<ConnectionEventArgs> Closed;

        event EventHandler<SignalREventArgs> SomeoneEntered;
        event EventHandler<SignalREventArgs> SomeoneLeft;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }

    public class MessagingService : IMessagingService
    {
        #region private members
        public HubConnection _hubConnection;
        public string _hubUrl;
        #endregion

        #region Initializer
        public void Init(string url, string token)
        {
            if (_hubConnection != null)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    /*
                        The access token function you provide is called before every HTTP request made by SignalR.
                        If you need to renew the token in order to keep the connection active
                        (because it may expire during the connection),
                        do so from within this function and return the updated token.
                        https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1#bearer-token-authentication
                     */
                    options.AccessTokenProvider = () =>
                    {
                        // verify access token here

                        // if not verified, refresh access token using refresh token

                        //Debug.WriteLine(token);
                        Debug.Assert(token != null);

                        return Task.FromResult(token);
                    };
                })
                .WithAutomaticReconnect(new[]
                {
                    // default: wait 0,2,10,30 seconds and try to reconnect
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(1)
                })
                .Build();

            // Handle Hub connection events
            _hubConnection.Closed += err =>
            {
                Closed?.Invoke(this, new ConnectionEventArgs($"Connection to SignalR Hub Closed. {err.Message}"));
                return Task.CompletedTask;
            };
            _hubConnection.Reconnecting += err =>
            {
                Reconnecting?.Invoke(this, new ConnectionEventArgs($"Try to reconnect to SignarR Hub. current error message: {err.Message}"));
                return Task.CompletedTask;
            };
            _hubConnection.Reconnected += connectionId =>
            {
                Reconnected?.Invoke(this, new ConnectionEventArgs($"Succesfully re-connected to SignalR hub. New Connectionid: {connectionId}"));
                return Task.CompletedTask;
            };

            // Handle Hub messages
            _hubConnection.On<string, string, int, int, DateTime, string>("ReceiveMessage", ReceiveMessage);

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
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public bool IsDisconnected => _hubConnection?.State == HubConnectionState.Disconnected;
        public bool IsConnecting => _hubConnection?.State == HubConnectionState.Connecting;
        public bool IsReconnecting => _hubConnection?.State == HubConnectionState.Reconnecting;
        public string ConnectionId => _hubConnection?.ConnectionId;
        #endregion

        #region Public Methods
        public async Task<int> SendMessageToRoomAsync(string roomId, string sender, string body)
        {
            if (!IsConnected)
                await ConnectAsync().ConfigureAwait(false);//SendMessageToRoomAsync

            return await _hubConnection.InvokeAsync<int>("SendMessageToRoomAsyc", body, roomId).ConfigureAwait(false);

            //Debug.WriteLine($"A message with {id} delivered to server");
        }
        public void ReceiveMessage(string senderName, string body, int messageId, int senderId, DateTime createdAt, string roomId = null)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(body, senderName, messageId, senderId, createdAt, roomId));

        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            try
            {
                Connecting?.Invoke(this, new ConnectionEventArgs($"Try to Connect to SignalR Hub"));

                await _hubConnection.StartAsync().ConfigureAwait(false);

                if (!IsConnected)
                    throw new InvalidOperationException($"Connecting to SignalR Hub failed.");

                Connected?.Invoke(this, new ConnectionEventArgs($"Successfully Connected to SignarR Hub. ConnectionId:{_hubConnection.ConnectionId}"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ConnectionFailed?.Invoke(this, new ConnectionEventArgs(ex.Message));
            }
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                Disconnecting?.Invoke(this, new ConnectionEventArgs($"Try to Disconnect\n{DateTime.UtcNow}\n{_hubConnection.ConnectionId}"));

                await _hubConnection.StopAsync();

                if (IsConnected)
                    throw new InvalidOperationException("Disconnection faild");

                Disconnected?.Invoke(this, new ConnectionEventArgs($"Disconnected\n{DateTime.UtcNow}"));
                Debug.WriteLine($"Disconnection completed\n{DateTime.UtcNow}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnection Failed: " + ex.Message);
                DisconnectionFailed?.Invoke(this, new ConnectionEventArgs("Disconnection Failed: " + ex.Message));
            }
        }
        public async Task JoinRoomAsync(string room, string user)
        {
            if (!IsConnected)
                await ConnectAsync().ConfigureAwait(false);//JoinRoomAsync

            await _hubConnection.SendAsync("AddToGroup", room, user);
        }
        public async Task LeaveRoomAsync(string room, string user)
        {
            if (!IsConnected)
                await ConnectAsync().ConfigureAwait(false);//LeaveRoomAsync

            await _hubConnection.SendAsync("RemoveFromGroup", room, user);
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
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        #endregion
    }
}
