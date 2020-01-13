using System;
using RingerStaff.ViewModels;

namespace RingerStaff.Models
{
    public class RoomModel : BaseViewModel
    {
        public string Id { get; set; }

        private string lastMessage;
        public string LastMessage
        {
            get => lastMessage;
            set => SetProperty(ref lastMessage, value);
        }

        private int unreadMessagesCount;
        public int UnreadMessagesCount
        {
            get => unreadMessagesCount;
            set => SetProperty(ref unreadMessagesCount, value);
        }

        private DateTime lastMessageArrivedAt;
        public DateTime LastMessageArrivedAt
        {
            get => lastMessageArrivedAt;
            set => SetProperty(ref lastMessageArrivedAt, value);
        }
    }
}
