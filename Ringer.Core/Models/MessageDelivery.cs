using System;

namespace Ringer.Core.Models
{
    public class MessageDelivery
    {
        /// <summary>
        /// the Id of this message delivery
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// the Id of Message which is associated with this delivery
        /// </summary>
        public int MessageId { get; }
        /// <summary>
        /// the id of sending user
        /// </summary>
        public int SenderId { get; }
        /// <summary>
        /// the id of receiving user
        /// </summary>
        public int ReceiverId { get; }
        /// <summary>
        /// the id of Chat room which this delevery was invoked
        /// </summary>
        public int RoomId { get; }
        /// <summary>
        /// the timestamp when the hub send to receiver.
        /// This not means the timestamp when the receiver actually received the message.
        /// If the receiver is not connected, despite of this timestamp
        /// </summary>
        public DateTime DeliverTime { get; }
        /// <summary>
        /// 받은 사람이 읽은 시간
        /// </summary>
        public DateTime AcknowledgeTime { get; }
        /// <summary>
        /// 전달 수단: 0.리얼타임, 1.푸시
        /// </summary>
        public DeliveryMethod Method { get; }

        public string ConnectionId { get; }
    }

    public enum DeliveryMethod
    {
        RealtimeMessaging,
        PushNotification
    }
}
