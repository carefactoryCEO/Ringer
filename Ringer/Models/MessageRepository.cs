using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Core.Data;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Types;
using Xamarin.Forms;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        ObservableCollection<MessageModel> Messages { get; }
        void AddLocalMessage(MessageModel message);
        void AddLoginMessage(MessageModel message);
        Task SaveToLocalDbAsync(MessageModel message);
        Task UpdateAsync(MessageModel message);
        Task AddMessageAsync(MessageModel message);
        Task LoadMessagesAsync(bool reset = false);
        void ClearLocalDb();
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;

        //public event PropertyChangedEventHandler PropertyChanged;

        public MessageRepository()
        {
            Messages = new ObservableCollection<MessageModel>();
            _localDbService = DependencyService.Resolve<ILocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();

            Debug.WriteLine(Constants.DbPath);
        }

        public ObservableCollection<MessageModel> Messages { get; set; }


        public void AddLoginMessage(MessageModel message)
        {
            Messages.Add(message);
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

            // 뷰에 표시
            AddLocalMessage(message);

            // 로컬 디비 저장
            await SaveToLocalDbAsync(message);

        }

        public void AddLocalMessage(MessageModel message)
        {
            Messages.Add(message);
            MessagingCenter.Send(this, "MessageAdded", message);
        }

        public async Task SaveToLocalDbAsync(MessageModel message)
        {
            if (message.ServerId != -1 && message.ServerId > App.LastServerMessageId)
                App.LastServerMessageId = message.ServerId;

            await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);
            await ModifyLastMessageAsync(message).ConfigureAwait(false);
        }

        private async Task ModifyLastMessageAsync(MessageModel message)
        {
            MessageModel lastMessage =
                //(Messages.Count > 0) ?
                Messages.LastOrDefault(m => m.RoomId == App.RoomId && m.Id < message.Id);
            //await _localDbService.GetLastMessageAsync(message.RoomId).ConfigureAwait(false);

            if (lastMessage != null)
            {
                if (lastMessage.SenderId == message.SenderId && Utilities.InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;

                    await _localDbService.SaveMessageAsync(lastMessage, update: true).ConfigureAwait(false);

                    Debug.WriteLine(Messages.Last().MessageTypes);
                }
            }
        }



        public async Task LoadMessagesAsync(bool reset = false)
        {
            if (!App.IsLoggedIn)
                return;

            if (reset)
                Device.BeginInvokeOnMainThread(async () => await Shell.Current.Navigation.PopToRootAsync());

            // 서버에서 긁어와 로컬 디비에 저장
            List<PendingMessage> pendingMessages = await _restService.PullPendingMessagesAsync().ConfigureAwait(false); // App.LastMessageId보다 큰 것만 긁어옴.

            int locallySavedLastServerMessageId = await _localDbService.GetLocallySavedLastServerMessageIdAsync(App.RoomId).ConfigureAwait(false);

            foreach (var pendingMessage in pendingMessages)
            {
                if (pendingMessage.Id <= locallySavedLastServerMessageId)
                    continue;

                var message = new MessageModel // App.LastMessageId보다 큰 것만 저 
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

            // 메모리 로드
            List<MessageModel> localDbMessages = await _localDbService.GetAllAsync().ConfigureAwait(false); // 로컬 디비 전부 불러옴.
            foreach (var message in localDbMessages)
            {
                // 메모리 로드된 메시지 중 Id 최대값이 추가하려는 메시지와 같으면 중단.
                // 즉 추가하려는 메시지가 이미 로드된 메시지들보다 Id값보다 더 커야 추가.
                // 즉 메모리에 있는 메시지보다 최신인 것만 추가한다는 얘기.
                if (Messages.Count > 0 && Messages[0].Id == message.Id)
                    break;

                Debug.WriteLine($"Add to Messages collection. message id: {message.Id}");
                Messages.Add(message);
            }

            if (reset)
                Device.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync($"//mappage/chatpage?room={App.RoomId}"));
        }
        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
