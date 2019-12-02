using System;

namespace Ringer.Core.Data
{
    public class PendingMessage
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SenderName { get; set; }
        public int SenderId { get; set; }
    }
}
