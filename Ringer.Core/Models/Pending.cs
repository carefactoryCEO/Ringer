namespace Ringer.Core.Models
{
    public class Pending
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
}