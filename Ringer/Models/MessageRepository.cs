using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Core.EventArgs;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Types;
using Xamarin.Forms;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        Task<List<MessageModel>> LoadRecentMessagesAsync(int take = 50);
        Task<List<MessageModel>> LoadMoreMessagesAsync(int take, int skip);
        Task AddMessageAsync(MessageModel message);

        void ClearLocalDb();

        event EventHandler<MessageModel> MessageAdded;
        event EventHandler<MessageModel> MessageUpdated;
    }

    public class MessageRepository : IMessageRepository
    {
        #region private fields
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;
        private readonly IMessaging _messaging;
        #endregion

        #region public Events
        public event EventHandler<MessageModel> MessageAdded;
        public event EventHandler<MessageModel> MessageUpdated;
        #endregion

        #region ctor
        public MessageRepository()
        {
            _localDbService = DependencyService.Resolve<ILocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();
            _messaging = DependencyService.Resolve<IMessaging>();

            _messaging.SomeoneEntered += SomeoneEntered;
            _messaging.SomeoneLeft += SomeoneLeft;
            _messaging.MessageReceived += MessageReceived;

            Debug.WriteLine(Constants.DbPath);
        }
        #endregion

        #region Messaging service event handlers
        private async void MessageReceived(object sender, MessageReceivedEventArgs e)
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

            Debug.WriteLine(App.RoomId);

            Utility.Trace(message.MessageTypes.ToString());
        }
        private async void SomeoneEntered(object sender, SignalREventArgs e)
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

            Utility.Trace(e.Message);
        }
        private async void SomeoneLeft(object sender, SignalREventArgs e)
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

            Utility.Trace(e.Message);
        }
        #endregion

        public async Task<List<MessageModel>> LoadRecentMessagesAsync(int take)
        {
            await PullMessages();

            return await _localDbService.GetMessagesAsync(take);
        }

        public Task<List<MessageModel>> LoadMoreMessagesAsync(int take, int skip)
        {
            return _localDbService.GetMessagesAsync(take, skip);
        }

        private async Task PullMessages()
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
                    MessageTypes = Utility.GetMediaAndDirectionType(p.Body, p.SenderId, App.UserId)
                }).ToArray();

            MessageModel lastSavedMessage = await _localDbService.GetLastMessageAsync(App.RoomId);
            MessageModel before;

            // set display type(leading/trailing)
            for (int i = 0; i < messages.Length; i++)
            {
                before = i > 0 ? messages[i - 1] : lastSavedMessage;

                if (before.SenderId == messages[i].SenderId && Utility.InSameMinute(messages[i].CreatedAt, before.CreatedAt))
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

        public async Task AddMessageAsync(MessageModel message)
        {
            // 직전 메시지 저장, 뷰 업데이트
            if (await UpdateLastMessageAsync(message).ConfigureAwait(false) is MessageModel updatedMessage)
                MessageUpdated?.Invoke(this, updatedMessage);

            // 메시지 저장, 뷰 추가, 스크롤
            if (await SaveToLocalDbAsync(message).ConfigureAwait(false) is MessageModel addedMessage)
                MessageAdded?.Invoke(this, addedMessage);
        }

        private async Task<MessageModel> UpdateLastMessageAsync(MessageModel message)
        {
            if (await _localDbService.GetLastMessageAsync(App.RoomId).ConfigureAwait(false) is MessageModel lastMessage)
            {
                if (lastMessage.SenderId == message.SenderId && Utility.InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
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

        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
