using Ringer.Core.Models;

namespace Ringer.Core.Data
{
    public class RegisterConsumerRequest
    {
        public User User { get; set; }
        public Device Device { get; set; }
    }

    public class RegisterConsumerResponse
    {
        public string Token { get; set; }
        public string RoomId { get; set; }
        public int UserId { get; set; }
        public bool Success { get; set; }
        public string UserName { get; set; }
    }
}
