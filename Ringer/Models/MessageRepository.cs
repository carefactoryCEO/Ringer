using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Ringer.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        ObservableCollection<Message> Messages { get; }

        Task AddMessageAsync(Message message);

        void AddLocalMessage(Message message);

        Task LoadMessagesAsync();
    }

    public class MessageRepository : IMessageRepository
    {
        private LocalDbService _database;

        public MessageRepository()
        {
            Messages = new ObservableCollection<Message>();
            _database = DependencyService.Resolve<LocalDbService>();
        }

        public ObservableCollection<Message> Messages { get; set; }

        public void AddLocalMessage(Message message)
        {
            Messages.Insert(0, message);
        }

        public Task AddMessageAsync(Message message)
        {
            AddLocalMessage(message);

            message.RoomId = App.CurrentRoomId;
            message.ReceivedAt = DateTime.UtcNow;

            Preferences.Set(message.RoomId, message.Id);

            return _database.SaveMessageAsync(message);
        }

        public async Task LoadMessagesAsync()
        {
            var messages = await _database.GetMessagesAsync();

            foreach (var message in messages)
                AddLocalMessage(message);
        }


    }
}
