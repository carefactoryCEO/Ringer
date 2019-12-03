using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
        public ObservableCollection<Message> Messages { get; set; }

        public MessageRepository()
        {
            Messages = new ObservableCollection<Message>();
        }

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

            return App.Database.SaveMessageAsync(message);
        }

        public async Task LoadMessagesAsync()
        {
            var messages = await App.Database.GetMessagesAsync();

            foreach (var message in messages)
                AddLocalMessage(message);
        }
    }
}
