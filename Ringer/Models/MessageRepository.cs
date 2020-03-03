using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Services;
using Xamarin.Forms;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        ObservableCollection<Message> Messages { get; }

        Task AddMessageAsync(Message message);

        Task SaveToLocalDbAsync(Message message);

        void AddLocalMessage(Message message);

        Task LoadMessagesAsync(bool reset = false);

        void ClearLocalDb();
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly ILocalDbService _localDbService;
        private readonly IRESTService _restService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageRepository()
        {
            Messages = new ObservableCollection<Message>();
            _localDbService = DependencyService.Resolve<ILocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();
        }

        public ObservableCollection<Message> Messages { get; set; }

        public void AddLocalMessage(Message message)
        {
            Messages.Insert(0, message);
        }

        double _totalMilliSeconds = 0;

        public async Task SaveToLocalDbAsync(Message message)
        {
            if (message.ServerId <= App.LastServerMessageId)
                return;

            var sw = new Stopwatch();
            sw.Start();

            var result = await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);

            if (result > 0)
                App.LastServerMessageId = message.ServerId;

            sw.Stop();
            Debug.WriteLine($"Save {message.Id} local db(ms): {sw.Elapsed.TotalMilliseconds}");
            _totalMilliSeconds += sw.Elapsed.TotalMilliseconds;

        }

        public Task AddMessageAsync(Message message)
        {
            if (message.ServerId <= App.LastServerMessageId)
                return Task.CompletedTask;

            // 일단 표시
            AddLocalMessage(message);

            // 로컬 디비 저장
            return SaveToLocalDbAsync(message);
        }

        public async Task LoadMessagesAsync(bool reset = false)
        {
            if (!App.IsLoggedIn)
                return;

            var stopwatch = new Stopwatch();

            if (reset)
                await Shell.Current.Navigation.PopToRootAsync();

            // 서버에서 긁어와 로컬 디비에 저장
            stopwatch.Start();
            var pendingMessages = await _restService.PullPendingMessagesAsync().ConfigureAwait(false); // App.LastMessageId보다 큰 것만 긁어옴.
            stopwatch.Stop();
            Debug.WriteLine($"Pull pending Messages from server time(ms) : {stopwatch.Elapsed.TotalMilliseconds}");

            var locallySavedLastServerMessageId = await _localDbService.GetLocallySavedLastServerMessageIdAsync(App.CurrentRoomId).ConfigureAwait(false);

            _totalMilliSeconds = 0;

            stopwatch.Restart();
            foreach (var pendingMessage in pendingMessages)
            {
                if (pendingMessage.Id <= locallySavedLastServerMessageId)
                    continue;

                await SaveToLocalDbAsync(new Message // App.LastMessageId보다 큰 것만 저 
                {
                    ServerId = pendingMessage.Id,
                    Body = pendingMessage.SenderName == App.UserName ? pendingMessage.Body : pendingMessage.SenderName + ": " + pendingMessage.Body,
                    Sender = pendingMessage.SenderName,
                    RoomId = App.CurrentRoomId,
                    SenderId = pendingMessage.SenderId,
                    CreatedAt = pendingMessage.CreatedAt,
                    ReceivedAt = DateTime.UtcNow
                }).ConfigureAwait(false);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Save to local DB time(ms) : {stopwatch.Elapsed.TotalMilliseconds}");
            Debug.WriteLine($"Sum db insert time(ms) : {_totalMilliSeconds}");

            //var tempMessages = new ObservableCollection<Message>();


            // 메모리 로드
            stopwatch.Restart();
            var localDbMessages = await _localDbService.GetMessagesAsync(true).ConfigureAwait(false); // 로컬 디비 전부 불러옴.
            foreach (var message in localDbMessages)
            {
                if (Messages.Count > 0 && Messages[0].Id == message.Id) // 메모리 로드된 메시지 중 Id 최대값이 추가하려는 메시지와 같으면 중단. 즉 추가하려는 메시지가 이미 로드된 메시지들보다 Id값보다 더 커야 추가. 즉 메모리에 있는 메시지보다 최신인 것만 추가한다는 얘기.
                    break;

                Debug.WriteLine($"Add to Messages collection. message id: {message.Id}");
                Messages.Add(message);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Add to Messages collection time(ms) : {stopwatch.Elapsed.TotalMilliseconds}");

            if (reset)
                await Shell.Current.GoToAsync($"//mappage/chatpage?room={App.CurrentRoomId}");
        }

        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
