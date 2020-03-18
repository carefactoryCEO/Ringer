using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Core.EventArgs;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Types;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface IMessaging : IMessagingService
    {
        HubConnection HubConnection { get; }
        ObservableCollection<MessageModel> Messages { get; }

        Task AddMessageAsync(MessageModel message);
        void ClearLocalDb();
        Task<string> InitAsync(string url, string token);
        void BufferMessages();
        Task InitMessagesAsync();

        event EventHandler<MessageModel> MessageAdded;
        event EventHandler<MessageModel> MessageUpdated;
    }

    public class Messaging : IMessaging
    {
        #region private members
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;
        public HubConnection _hubConnection;
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

        public event EventHandler<MessageModel> MessageAdded;
        public event EventHandler<MessageModel> MessageUpdated;
        #endregion

        #region public properties
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public bool IsDisconnected => _hubConnection?.State == HubConnectionState.Disconnected;
        public bool IsConnecting => _hubConnection?.State == HubConnectionState.Connecting;
        public bool IsReconnecting => _hubConnection?.State == HubConnectionState.Reconnecting;
        public string ConnectionId => _hubConnection?.ConnectionId;
        public HubConnection HubConnection => _hubConnection;
        public ObservableCollection<MessageModel> Messages { get; }
        #endregion

        #region Constructor and Initializer
        public Messaging()
        {
            _localDbService = DependencyService.Resolve<ILocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();

            Messages = new ObservableCollection<MessageModel>();

            SomeoneEntered += OnSomeoneEntered;
            SomeoneLeft += OnSomeoneLeft;
            MessageReceived += OnMessageReceived;
        }
        public void Init(string url, string token)
        {
            if (_hubConnection != null)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    /** The access token function you provide is called before every HTTP request made by SignalR.
                     *  If you need to renew the token in order to keep the connection active (because it may expire during the connection),
                     *  do so from within this function and return the updated token.
                     *  https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1#bearer-token-authentication
                     * */
                    options.AccessTokenProvider = () =>
                    {
                        // verify access token here
                        // if not verified, refresh access token using refresh token

                        Utilities.Trace($"connection token: {token}");

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
            _hubConnection.On<string, string, int, int, DateTime, string>("ReceiveMessage", (senderName, body, messageId, senderId, createdAt, roomId) => MessageReceived?.Invoke(this, new MessageReceivedEventArgs(body, senderName, messageId, senderId, createdAt, roomId)));
            _hubConnection.On<string>("Entered", user => SomeoneEntered?.Invoke(this, new SignalREventArgs($"{user} entered.", user)));
            _hubConnection.On<string>("Left", user => SomeoneLeft?.Invoke(this, new SignalREventArgs($"{user} left.", user)));
        }
        public async Task<string> InitAsync(string url, string token)
        {
            Init(url, token);

            await Task.WhenAll(new Task[]
            {
                ConnectAsync(),
                InitMessagesAsync()

            }).ConfigureAwait(false);

            return _hubConnection.ConnectionId;
        }
        #endregion

        #region private Methods
        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // 내가 방금 보낸 건지 체크
            if (e.SenderId == App.UserId)
            {
                if (await _localDbService.GetSentMessageAsync(App.RoomId) is MessageModel sentMessage)
                {
                    App.LastServerMessageId = sentMessage.ServerId = e.MessageId;
                    await _localDbService.UpdateMessageAsync(sentMessage);

                    return;
                }
            }

            var message = new MessageModel
            {
                ServerId = e.MessageId,
                RoomId = App.RoomId,
                Body = e.Body,
                Sender = e.SenderName,
                SenderId = e.SenderId,
                CreatedAt = e.CreatedAt,
                ReceivedAt = DateTime.UtcNow,
                MessageTypes = Utilities.GetMediaAndDirectionType(e.Body, e.SenderId, App.UserId)
            };

            await AddMessageAsync(message);
        }
        private async void OnSomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender != App.UserName)
                await AddMessageAsync(new MessageModel
                {
                    Body = $"{e.Sender} 님이 들어왔습니다.",
                    Sender = Constants.System,
                    MessageTypes = MessageTypes.EntranceNotice,
                    CreatedAt = DateTime.UtcNow,
                    ReceivedAt = DateTime.UtcNow
                }).ConfigureAwait(false);

            Utilities.Trace(e.Message);
        }
        private async void OnSomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender != App.UserName)
                await AddMessageAsync(new MessageModel
                {
                    Body = $"{e.Sender} 님이 나갔습니다.",
                    Sender = Constants.System,
                    MessageTypes = MessageTypes.EntranceNotice,
                    CreatedAt = DateTime.UtcNow,
                    ReceivedAt = DateTime.UtcNow
                }).ConfigureAwait(false);

            Utilities.Trace(e.Message);
        }
        private async Task<MessageModel> UpdateLastMessageAsync(MessageModel message)
        {
            if (await _localDbService.GetLastMessageAsync(App.RoomId).ConfigureAwait(false) is MessageModel lastMessage)
            {
                if (lastMessage.SenderId == message.SenderId && Utilities.InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    // 메시지 타입 수정
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;

                    // 디비 저장
                    return await _localDbService.UpdateMessageAsync(lastMessage).ConfigureAwait(false);
                }
            }

            return null;
        }
        private async Task<MessageModel> SaveToLocalDbAsync(MessageModel message)
        {
            if (message.ServerId != -1 && message.ServerId > App.LastServerMessageId)
                App.LastServerMessageId = message.ServerId;

            // 디비 저장
            return await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);
        }
        private async Task PullMessagesAsync()
        {
            // App.LastMessageId보다 큰 것만 긁어옴.
            List<PendingMessage> pendingMessages = await _restService.PullPendingMessagesAsync(App.RoomId, App.LastServerMessageId, App.Token).ConfigureAwait(false);

            if (pendingMessages.Count > 0)
                App.LastServerMessageId = pendingMessages.Last().Id;
            else
                return;

            // 임시 array
            // Media Type and direction selected
            var messages = pendingMessages
                .OrderBy(p => p.CreatedAt)
                .Select(p => new MessageModel
                {
                    ServerId = p.Id,
                    Body = p.Body,
                    Sender = p.SenderName,
                    RoomId = App.RoomId,
                    SenderId = p.SenderId,
                    CreatedAt = p.CreatedAt,
                    ReceivedAt = DateTime.UtcNow,
                    MessageTypes = Utilities.GetMediaAndDirectionType(p.Body, p.SenderId, App.UserId)
                }).ToArray();

            MessageModel lastSavedMessage = await _localDbService.GetLastMessageAsync(App.RoomId);
            MessageModel before;

            // set display type(leading/trailing)
            for (int i = 0; i < messages.Length; i++)
            {
                before = i > 0 ? messages[i - 1] : lastSavedMessage;

                if (before.SenderId == messages[i].SenderId && Utilities.InSameMinute(messages[i].CreatedAt, before.CreatedAt))
                {
                    // 메시지 타입 수정
                    before.MessageTypes ^= MessageTypes.Trailing;
                    messages[i].MessageTypes ^= MessageTypes.Leading;
                }

            }

            await _localDbService.UpdateMessageAsync(lastSavedMessage);

            foreach (var m in messages)
            {
                await _localDbService.SaveMessageAsync(m);
                Debug.WriteLine($"server id:{m.ServerId}, message types: {m.MessageTypes}");
            }

        }
        #endregion

        #region Public Methods
        public async Task InitMessagesAsync()
        {
            if (Messages.Any())
                Messages.Clear();

            await PullMessagesAsync();

            var messages = await _localDbService.GetMessagesAsync(Constants.MessageCount);
            foreach (var m in messages)
                Messages.Add(m);
        }
        public async void BufferMessages()
        {
            var messages = await _localDbService.GetMessagesAsync(Constants.MessageCount, Messages.Count);

            foreach (var m in messages)
                Messages.Insert(messages.IndexOf(m), m);
        }

        public async Task AddMessageAsync(MessageModel message)
        {
            // 직전 메시지 저장, 뷰 업데이트
            if (await UpdateLastMessageAsync(message).ConfigureAwait(false) is MessageModel updatedMessage)
                MessageUpdated?.Invoke(this, updatedMessage);

            // 메시지 저장, 뷰 추가, 스크롤
            if (await SaveToLocalDbAsync(message).ConfigureAwait(false) is MessageModel addedMessage)
            {
                MessageAdded?.Invoke(this, addedMessage);
                Messages.Add(addedMessage);
            }
        }
        public async Task<int> SendMessageToRoomAsync(string roomId, string sender, string body)
        {
            if (!IsConnected)
                await ConnectAsync().ConfigureAwait(false);//SendMessageToRoomAsync

            return await _hubConnection.InvokeAsync<int>("SendMessageToRoomAsyc", body, roomId).ConfigureAwait(false);
        }

        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
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
        public async Task DisconnectAsync(string room, string user)
        {
            if (!IsConnected)
                return;

            await LeaveRoomAsync(room, user);
            await DisconnectAsync();
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
    }
}
