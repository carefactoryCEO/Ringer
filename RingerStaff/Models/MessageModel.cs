using System;
using RingerStaff.Types;
using RingerStaff.ViewModels;

namespace RingerStaff.Models
{
    public class MessageModel : BaseViewModel
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string RoomId { get; set; }
        public string Body { get; set; }
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
