using System.Collections;
using System.Collections.Generic;

namespace Ringer.Core.Models
{
    public class Message
    {
        public Message() { }

        public Message(string content, string sender = "system") : this()
        {
            Content = content;
            Sender = sender;
        }

        public int Id { get; set; }

        public string Content { get; set; }

        public string Sender { get; set; } // sender
        public int SenderId { get; set; }

        public string Room { get; set; }
        public int RoomId { get; set; }

    }
}
