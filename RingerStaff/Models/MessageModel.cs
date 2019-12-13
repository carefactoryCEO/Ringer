using System;
using RingerStaff.Types;
using RingerStaff.ViewModels;

namespace RingerStaff.Models
{
    public class MessageModel : BaseViewModel
    {
        private string body;
        private string sender;
        private MessageTypes messageTypes;
        private int unreadCount;

        public string Body { get => body; set => SetProperty(ref body, value); }
        public string Sender { get => sender; set => SetProperty(ref sender, value); }
        public MessageTypes MessageTypes { get => messageTypes; set => SetProperty(ref messageTypes, value); }
        public int UnreadCount { get => unreadCount; set => SetProperty(ref unreadCount, value); }

    }
}
