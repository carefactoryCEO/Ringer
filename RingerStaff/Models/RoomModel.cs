using System;
using System.Dynamic;
using RingerStaff.ViewModels;

namespace RingerStaff.Models
{
    public class RoomModel : BaseViewModel
    {
        private string description;
        private string lastMessage;
        private string roomId;
        private DateTime lastMessageArrivedAt;
        private int unreadMessagesCount;

        public string Description { get => description; set => SetProperty(ref description, value); }
        public string LastMessage { get => lastMessage; set => SetProperty(ref lastMessage, value); }
        public string RoomId { get => roomId; set => SetProperty(ref roomId, value); }
        public DateTime LastMessageArrivedAt { get => lastMessageArrivedAt; set => SetProperty(ref lastMessageArrivedAt, value); }
        public int UnreadMessagesCount { get => unreadMessagesCount; set => SetProperty(ref unreadMessagesCount, value); }
    }
}
