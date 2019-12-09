using System;
using SQLite;

namespace Ringer.Models
{
    public class Message
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        public string RoomId { get; set; }

        public string Body { get; set; }

        public string Sender { get; set; }

        public int SenderId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ReceivedAt { get; set; }
    }
}
