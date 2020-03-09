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

        Task AddMessageAsync(MessageModel message);

        Task SaveToLocalDbAsync(MessageModel message);

        Task UpdateAsync(MessageModel message);

        void AddLocalMessage(MessageModel message);

        void AddLoginMessage(MessageModel message);

        Task LoadMessagesAsync(bool reset = false);

        Task<MessageModel> ModifyLastMessageAsync(MessageModel message);

        void ClearLocalDb();
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageRepository()
        {
            Messages = new ObservableCollection<MessageModel>();
            _localDbService = DependencyService.Resolve<ILocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();

            Debug.WriteLine(Constants.DbPath);
        }

        public ObservableCollection<MessageModel> Messages { get; set; }

        public async Task<MessageModel> ModifyLastMessageAsync(MessageModel message)
        {
            MessageModel lastMessage = (Messages.Count > 0) ? Messages.Where(m => m.RoomId == App.CurrentRoomId).OrderByDescending(m => m.CreatedAt).FirstOrDefault() : await _localDbService.GetLastMessageAsync();

            if (lastMessage != null)
            {
                if (lastMessage.SenderId == message.SenderId && Utilities.InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;

                    await _localDbService.SaveMessageAsync(lastMessage, update: true);
                }
            }

            return message;
        }

        public void AddLocalMessage(MessageModel message)
        {
            Messages.Add(message);
        }

        public void AddLoginMessage(MessageModel message)
        {
            Messages.Add(message);
        }

        double _totalMilliSeconds = 0;

        public async Task SaveToLocalDbAsync(MessageModel message)
        {
            if (message.ServerId <= App.LastServerMessageId)
                return;

            App.LastServerMessageId = message.ServerId;

            await ModifyLastMessageAsync(message);
            await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);
        }

        public async Task UpdateAsync(MessageModel message)
        {
            await _localDbService.SaveMessageAsync(message, update: true);
        }

        public async Task AddMessageAsync(MessageModel message)
        {
            if (message.ServerId <= App.LastServerMessageId)
                return;

            // 로컬 디비 저장
            await SaveToLocalDbAsync(message);

            // 일단 표시
            AddLocalMessage(message);
        }

        public async Task LoadMessagesAsync(bool reset = false)
        {
            if (!App.IsLoggedIn)
                return;

            if (reset)
                Device.BeginInvokeOnMainThread(async () => await Shell.Current.Navigation.PopToRootAsync());

            // 서버에서 긁어와 로컬 디비에 저장
            List<PendingMessage> pendingMessages = await _restService.PullPendingMessagesAsync().ConfigureAwait(false); // App.LastMessageId보다 큰 것만 긁어옴.

            int locallySavedLastServerMessageId = await _localDbService.GetLocallySavedLastServerMessageIdAsync(App.CurrentRoomId).ConfigureAwait(false);

            foreach (var pendingMessage in pendingMessages)
            {
                if (pendingMessage.Id <= locallySavedLastServerMessageId)
                    continue;

                var message = new MessageModel // App.LastMessageId보다 큰 것만 저 
                {
                    ServerId = pendingMessage.Id,
                    Body = pendingMessage.Body,
                    Sender = pendingMessage.SenderName,
                    RoomId = App.CurrentRoomId,
                    SenderId = pendingMessage.SenderId,
                    CreatedAt = pendingMessage.CreatedAt,
                    ReceivedAt = DateTime.UtcNow,
                    MessageTypes = (pendingMessage.SenderId == App.UserId) ? MessageTypes.Outgoing : MessageTypes.Incomming
                };

                message.MessageTypes |= MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing;

                await SaveToLocalDbAsync(message).ConfigureAwait(false);
            }

            // 메모리 로드
            List<MessageModel> localDbMessages = await _localDbService.GetMessagesAsync().ConfigureAwait(false); // 로컬 디비 전부 불러옴.
            foreach (var message in localDbMessages)
            {
                if (Messages.Count > 0 && Messages[0].Id == message.Id) // 메모리 로드된 메시지 중 Id 최대값이 추가하려는 메시지와 같으면 중단. 즉 추가하려는 메시지가 이미 로드된 메시지들보다 Id값보다 더 커야 추가. 즉 메모리에 있는 메시지보다 최신인 것만 추가한다는 얘기.
                    break;

                Debug.WriteLine($"Add to Messages collection. message id: {message.Id}");
                Messages.Add(message);
            }

            if (reset)
                Device.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync($"//mappage/chatpage?room={App.CurrentRoomId}"));
        }

        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
