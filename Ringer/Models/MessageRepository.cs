using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Core.EventArgs;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Types;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        Task<List<MessageModel>> GetMessagesAsync(int skip = 0, int take = 50);
        Task AddMessageAsync(MessageModel message);

        void AddLoginMessage(MessageModel message);
        Task SaveToLocalDbAsync(MessageModel message);
        Task UpdateAsync(MessageModel message);
        void ClearLocalDb();

        event EventHandler<MessageModel> MessageAdded;
        event EventHandler<MessageModel> MessageUpdated;
    }

    public class MessageRepository : IMessageRepository
    {
        #region private fields
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;
        private readonly IMessagingService _messagingService;
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
            _messagingService = DependencyService.Resolve<IMessagingService>();

            _messagingService.SomeoneEntered += SomeoneEntered;
            _messagingService.SomeoneLeft += SomeoneLeft;
            _messagingService.MessageReceived += MessageReceived;

            Debug.WriteLine(Constants.DbPath);
        }
        #endregion

        #region Messaging service event handlers
        private async void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // 내가 방금 보낸 건지 체크
            if (e.SenderId == App.UserId)
            {
                MessageModel sentModel = await _localDbService.GetSentMessageAsync(App.RoomId);
                if (sentModel != null)
                {
                    sentModel.ServerId = e.MessageId;
                    await _localDbService.SaveMessageAsync(sentModel, true);

                    return;
                }
            }

            MessageTypes messageTypes = Utilities.SetMessageTypes(e.Body, e.SenderId, App.UserId);
            var message = new MessageModel
            {
                ServerId = e.MessageId,
                RoomId = App.RoomId,
                Body = e.Body,
                Sender = e.SenderName,
                SenderId = e.SenderId,
                CreatedAt = e.CreatedAt,
                ReceivedAt = DateTime.UtcNow,
                MessageTypes = messageTypes
            };

            await AddMessageAsync(message);

            Debug.WriteLine(App.RoomId);

            Utilities.Trace(message.MessageTypes.ToString());
        }

        private async void SomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender != App.UserName)
                await AddMessageAsync(new MessageModel { Body = $"{e.Sender}님이 들어왔습니다.", Sender = Constants.System, MessageTypes = MessageTypes.EntranceNotice }).ConfigureAwait(false);

            Utilities.Trace(e.Message);
        }

        private async void SomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender != App.UserName)
                await AddMessageAsync(new MessageModel { Body = $"{e.Sender}님이 나갔습니다.", Sender = Constants.System, MessageTypes = MessageTypes.EntranceNotice }).ConfigureAwait(false);

            Utilities.Trace(e.Message);
        }
        #endregion

        public async Task<List<MessageModel>> GetMessagesAsync(int skip = 0, int take = 50)
        {
            await PullMessages();

            return await _localDbService.GetMessagesAsync(skip, take);
        }

        private async Task PullMessages()
        {
            List<PendingMessage> pendingMessages = await _restService.PullPendingMessagesAsync(App.LastServerMessageId).ConfigureAwait(false); // App.LastMessageId보다 큰 것만 긁어옴.

            foreach (var pendingMessage in pendingMessages)
            {
                var message = new MessageModel
                {
                    ServerId = pendingMessage.Id,
                    Body = pendingMessage.Body,
                    Sender = pendingMessage.SenderName,
                    RoomId = App.RoomId,
                    SenderId = pendingMessage.SenderId,
                    CreatedAt = pendingMessage.CreatedAt,
                    ReceivedAt = DateTime.UtcNow,
                    MessageTypes = Utilities.SetMessageTypes(pendingMessage.Body, pendingMessage.SenderId, App.UserId)
                };

                await SaveToLocalDbAsync(message).ConfigureAwait(false);
            }
        }

        public void AddLoginMessage(MessageModel message)
        {
            //Messages.Add(message);
        }

        public async Task UpdateAsync(MessageModel message)
        {
            if (message.ServerId > App.LastServerMessageId)
                App.LastServerMessageId = message.ServerId;

            await _localDbService.SaveMessageAsync(message, update: true);
        }
        public async Task AddMessageAsync(MessageModel message)
        {
            if (message.ServerId != -1 && message.ServerId <= App.LastServerMessageId)
                return;

            // 로컬 디비 저장
            await ModifyLastMessageAsync(message).ConfigureAwait(false);
            await SaveToLocalDbAsync(message);
        }

        public async Task SaveToLocalDbAsync(MessageModel message)
        {
            if (message.ServerId != -1 && message.ServerId > App.LastServerMessageId)
                App.LastServerMessageId = message.ServerId;

            var addedMessage = await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);

            MessageAdded?.Invoke(this, addedMessage);
        }

        private async Task ModifyLastMessageAsync(MessageModel message)
        {
            MessageModel lastMessage =
                await _localDbService.GetLastMessageAsync(App.RoomId).ConfigureAwait(false);

            if (lastMessage != null)
            {
                if (lastMessage.SenderId == message.SenderId && Utilities.InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;

                    var updatedMessage = await _localDbService.SaveMessageAsync(lastMessage, update: true).ConfigureAwait(false);

                    MessageUpdated?.Invoke(this, updatedMessage);
                }
            }
        }
        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
