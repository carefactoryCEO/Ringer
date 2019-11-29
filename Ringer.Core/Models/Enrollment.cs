namespace Ringer.Core.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }

        public User User { get; set; }
        public Room Room { get; set; }
    }
}
