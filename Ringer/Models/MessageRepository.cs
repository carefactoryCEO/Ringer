using System.Collections.ObjectModel;
using Ringer.Core.Models;

namespace Ringer.Models
{
    public interface IMessageRepository
    {
        ObservableCollection<Message> Messages { get; }

        void AddMessage(Message message);

        void AddLocalMessage(Message message);
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
            AddMessage(message);
        }

        public void AddMessage(Message message)
        {
            Messages.Insert(0, message);
        }
    }
}
