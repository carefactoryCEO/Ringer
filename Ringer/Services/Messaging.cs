using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.LocalNotification;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Core.EventArgs;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Types;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface IMessaging : IMessagingService
    {
        HubConnection HubConnection { get; }
        ObservableCollection<MessageModel> Messages { get; }

        Task AddMessageAsync(MessageModel message);
        Task<string> InitAsync(string url, string token);
        void BufferMessages();
        Task InitMessagesAsync();

        event EventHandler<MessageModel> MessageAdded;
        event EventHandler<MessageModel> MessageUpdated;
        event EventHandler<FetchingState> FetchingStateChanged;
        event EventHandler<MessageModel[]> MessagesFetched;

        Task FetchRemoteMessagesAsync();
        Task EnsureConnected();
        Task Clear();
    }

    public class Messaging : IMessaging
    {
        #region private members
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;
        public HubConnection _hubConnection;
        private bool isPulling;
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
        public event EventHandler<FetchingState> FetchingStateChanged;
        public event EventHandler<MessageModel[]> MessagesFetched;
        #endregion

        #region public properties
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public bool IsDisconnected => _hubConnection?.State == HubConnectionState.Disconnected;
        public bool IsConnecting => _hubConnection?.State == HubConnectionState.Connecting;
        public bool IsReconnecting => _hubConnection?.State == HubConnectionState.Reconnecting;
        public string ConnectionId => _hubConnection?.ConnectionId ?? Constants.DisconnectedString;
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

                        Utility.Trace($"connection token: {token}");

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
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            Init(url, token);

            FetchingStateChanged?.Invoke(this, FetchingState.Fetching);

            await Task.WhenAll(new Task[]
            {
                InitMessagesAsync(),
                ConnectAsync()
            });

            FetchingStateChanged?.Invoke(this, FetchingState.Finished);

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
                MessageTypes = Utility.GetMediaAndDirectionType(e.Body, e.SenderId, App.UserId)
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
                    ReceivedAt = DateTime.UtcNow,
                    RoomId = App.RoomId
                });

            Utility.Trace(e.Message);
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
                    ReceivedAt = DateTime.UtcNow,
                    RoomId = App.RoomId
                });

            Utility.Trace(e.Message);
        }
        private async Task<MessageModel> UpdateLastMessageAsync(MessageModel currentMessage)
        {
            if (await _localDbService.GetLastMessageAsync(App.RoomId).ConfigureAwait(false) is MessageModel lastMessage)
            {
                if (lastMessage.SenderId == currentMessage.SenderId && Utility.InSameMinute(currentMessage.CreatedAt, lastMessage.CreatedAt))
                {
                    // 메시지 타입 수정
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    currentMessage.MessageTypes ^= MessageTypes.Leading;

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
        private async Task<MessageModel[]> PullRemoteMessagesAsync()
        {
            if (isPulling)
                return null;

            isPulling = true;

            if (await _restService.PullPendingMessagesAsync(App.RoomId, App.LastServerMessageId, App.Token) is List<PendingMessage> pendingMessages && pendingMessages.Any())
            {
                // MessageModel로 변환
                MessageModel[] messages = pendingMessages
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
                        MessageTypes = Utility.GetMediaAndDirectionType(p.Body, p.SenderId, App.UserId)
                    }).ToArray();

                // MessagesTypes 수정
                MessageModel lastSavedMessage = await _localDbService.GetLastMessageAsync(App.RoomId);
                MessageModel before;

                // set display type(leading/trailing)
                for (int i = 0; i < messages.Length; i++)
                {
                    before = i > 0 ? messages[i - 1] : lastSavedMessage;

                    if (before != null && before.SenderId == messages[i].SenderId && Utility.InSameMinute(messages[i].CreatedAt, before.CreatedAt))
                    {
                        // 메시지 타입 수정
                        before.MessageTypes ^= MessageTypes.Trailing;
                        messages[i].MessageTypes ^= MessageTypes.Leading;
                    }
                }

                if (lastSavedMessage != null)
                {
                    await _localDbService.UpdateMessageAsync(lastSavedMessage);
                    MessageUpdated?.Invoke(this, lastSavedMessage);
                }

                isPulling = false;
                return messages;
            }

            isPulling = false;
            return null;
        }
        #endregion

        #region Public Methods
        public async Task AddMessageAsync(MessageModel message)
        {
            // 직전 메시지 저장, 뷰 업데이트
            if (await UpdateLastMessageAsync(message) is MessageModel updatedMessage)
                MessageUpdated?.Invoke(this, updatedMessage);

            // 메시지 저장, 뷰 추가, 스크롤
            if (await SaveToLocalDbAsync(message) is MessageModel addedMessage)
            {
                Messages.Add(addedMessage);
                MessageAdded?.Invoke(this, addedMessage);
            }

            if (!Utility.IsChatActive && message.SenderId != App.UserId && !Utility.AndroidCameraActivated)
            {
                var notification = new NotificationRequest
                {
                    //BadgeNumber = 1,
                    NotificationId = ++App.LocalNotificationId,
                    Title = message.Sender,
                    Description = message.Body,
                    ReturningData = $"server id: {message.ServerId}", // Returning data when tapped on notification.
                    //NotifyTime = DateTime.Now.AddSeconds(5), // Used for Scheduling local notification, if not specified notification will show immediately.
                    //Sound = Device.RuntimePlatform == Device.Android ? "filling_your_inbox" : "filling_your_inbox.m4r",
                    //Sound = Device.RuntimePlatform == Device.Android ? "good_things_happen" : "good_things_happen.mp3",
                };

                NotificationCenter.Current.Show(notification);
                Vibration.Vibrate();
            }
        }
        public async Task InitMessagesAsync()
        {
            Messages.Clear();

            // 서버에서 당겨와서 디비 저장
            if (await PullRemoteMessagesAsync() is MessageModel[] messages)
            {
                //foreach (var m in messages.TakeLast(Constants.MessageCount))
                //    Messages.Add(m);

                foreach (var message in messages)
                {
                    await SaveToLocalDbAsync(message);
                }
            }

            // 디비에서 불러와서 메모리 로드
            foreach (var m in await _localDbService.GetMessagesAsync(Constants.MessageCount))
                Messages.Add(m);
        }
        public async Task FetchRemoteMessagesAsync()
        {
            //if (App.LastConnectionId == ConnectionId)
            //    return;

            if (await PullRemoteMessagesAsync() is MessageModel[] messages)
            {
                FetchingStateChanged?.Invoke(this, FetchingState.Fetching);

                foreach (var message in messages)
                {
                    if (!await _localDbService.HasServerIdAsync(message))
                    {
                        await SaveToLocalDbAsync(message);
                        Messages.Add(message);
                    }
                }
                MessagesFetched?.Invoke(this, messages);
                FetchingStateChanged?.Invoke(this, FetchingState.Finished);
            }
        }
        public async void BufferMessages()
        {
            var messages = await _localDbService.GetMessagesAsync(Constants.MessageCount, Messages.Count);

            foreach (var m in messages)
                Messages.Insert(messages.IndexOf(m), m);
        }

        public async Task SendMessageToRoomAsync(string roomId, string sender, string body)
        {
            await EnsureConnected();

            await _hubConnection.InvokeAsync("SendMessageToRoomAsyc", body, roomId).ConfigureAwait(false);
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

                Disconnected?.Invoke(this, new ConnectionEventArgs($"Disconnection completed\n{DateTime.UtcNow}"));

            }
            catch (Exception ex)
            {
                DisconnectionFailed?.Invoke(this, new ConnectionEventArgs(ex.Message));
            }
        }
        public async Task EnsureConnected()
        {
            if (IsConnected)
                return;

            await ConnectAsync().ConfigureAwait(false);
        }

        public async Task JoinRoomAsync(string room, string user)
        {
            await EnsureConnected();

            await _hubConnection.SendAsync("AddToGroup", room, user);
        }
        public async Task LeaveRoomAsync(string room, string user)
        {
            await EnsureConnected();

            await _hubConnection.SendAsync("RemoveFromGroup", room, user);
        }
        public async Task Clear()
        {
            Messages.Clear();
            await _localDbService.ResetMessagesAsync();
        }
        #endregion
    }
}
