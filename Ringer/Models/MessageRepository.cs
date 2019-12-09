using System;
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

        void AddLocalMessage(Message message);

        Task LoadMessagesAsync(bool reset = false);

        void ClearLocalDb();
    }

    public class MessageRepository : IMessageRepository
    {
        private LocalDbService _localDbService;
        private IRESTService _restService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageRepository()
        {
            Messages = new ObservableCollection<Message>();
            _localDbService = DependencyService.Resolve<LocalDbService>();
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
            if (message.Id <= App.LastMessageId)
                return;

            var sw = new Stopwatch();
            sw.Start();

            // 로컬 디비 저장
            message.RoomId = App.CurrentRoomId;
            message.ReceivedAt = DateTime.UtcNow;

            var result = await _localDbService.SaveMessageAsync(message).ConfigureAwait(false);

            if (result > 0)
                App.LastMessageId = message.Id;

            sw.Stop();
            Debug.WriteLine($"Save message {message.Id} takes {sw.Elapsed.TotalMilliseconds}.");
            _totalMilliSeconds += sw.Elapsed.TotalMilliseconds;

        }

        public Task AddMessageAsync(Message message)
        {
            if (message.Id <= App.LastMessageId)
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
            var pendingMessages = await _restService.PullPendingMessagesAsync(); // App.LastMessageId보다 큰 것만 긁어옴.
            stopwatch.Stop();
            Debug.WriteLine($"Pull pending Messages from server time : {stopwatch.Elapsed.TotalSeconds}");

            var dblastid = await _localDbService.GetLastMessageIndexAsync(App.CurrentRoomId);

            _totalMilliSeconds = 0;

            stopwatch.Restart();
            foreach (var pendingMessage in pendingMessages)
            {
                if (pendingMessage.Id <= dblastid)
                    continue;

                Debug.WriteLine($"Save to local DB. message id: {pendingMessage.Id}");

                await SaveToLocalDbAsync(new Message // App.LastMessageId보다 큰 것만 저 
                {
                    Id = pendingMessage.Id,
                    Body = pendingMessage.SenderName == App.UserName ? pendingMessage.Body : pendingMessage.SenderName + ": " + pendingMessage.Body,
                    Sender = pendingMessage.SenderName,
                    SenderId = pendingMessage.SenderId,
                    CreatedAt = pendingMessage.CreatedAt
                });
            }
            stopwatch.Stop();
            Debug.WriteLine($"Save to local DB time : {stopwatch.Elapsed.TotalSeconds}");
            Debug.WriteLine($"total milliseconds : {_totalMilliSeconds}");

            //var tempMessages = new ObservableCollection<Message>();


            // 메모리 로드
            stopwatch.Restart();
            var localDbMessages = await _localDbService.GetMessagesAsync(true); // 로컬 디비 전부 불러옴.
            foreach (var message in localDbMessages)
            {
                if (Messages.Count > 0 && Messages[0].Id == message.Id) // 메모리 로드된 메시지 중 Id 최대값이 추가하려는 메시지와 같으면 중단. 즉 추가하려는 메시지가 이미 로드된 메시지들보다 Id값보다 더 커야 추가. 즉 메모리에 있는 메시지보다 최신인 것만 추가한다는 얘기.
                    break;

                Debug.WriteLine($"Add to Messages collection. message id: {message.Id}");
                Messages.Add(message);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Add to Messages collection time : {stopwatch.Elapsed.TotalSeconds}");

            if (reset)
                await Shell.Current.GoToAsync($"//mappage/chatpage?room={App.CurrentRoomId}");
        }

        public void ClearLocalDb()
        {
            _localDbService.ResetMessagesAsync();
        }
    }
}
