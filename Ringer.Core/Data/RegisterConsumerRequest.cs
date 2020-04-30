using Ringer.Core.Models;

namespace Ringer.Core.Data
{
    public class ConsumerAuthRequest
    {
        public User User { get; set; }
        public Device Device { get; set; }
    }

    public class ConsumerAuthResponse
    {
        public string Token { get; set; }
        public string RoomId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool Success { get; set; }
        public bool RequireLogin { get; set; }
        public bool IsAlreadyRegistered { get; set; }
        public string ErrorMessage { get; set; }
    }
}
