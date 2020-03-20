using System;
using Ringer.Types;
using Ringer.ViewModels;
using SQLite;

namespace Ringer.Models
{
    public class MessageModel : BaseViewModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ServerId { get; set; }
        [Indexed]
        public string RoomId { get; set; }

        private string body;
        public string Body
        {
            get => body;
            set => SetProperty(ref body, value);
        }
        public string Sender { get; set; }
        public int SenderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }

        private MessageTypes messageTypes;
        public MessageTypes MessageTypes
        {
            get => messageTypes;
            set => SetProperty(ref messageTypes, value);
        }

        private int unreadCount;
        public int UnreadCount
        {
            get => unreadCount;
            set => SetProperty(ref unreadCount, value);
        }
    }
}
