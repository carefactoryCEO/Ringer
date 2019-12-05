using System;

namespace Ringer.Core.EventArgs
{
    public class MessageReceivedEventArgs
    {
        public MessageReceivedEventArgs(string body, string senderName, int messageId, int senderId, DateTime createdAt)
        {
            Body = body;
            SenderName = senderName;
            MessageId = messageId;
            SenderId = senderId;
            CreatedAt = createdAt;
        }

        public string Body { get; }
        public string SenderName { get; }
        public int MessageId { get; }
        public int SenderId { get; }
        public DateTime CreatedAt { get; }
    }
}