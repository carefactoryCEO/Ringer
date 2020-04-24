using System;
using System.ComponentModel.DataAnnotations;

namespace Ringer.Core.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrolledAt { get; set; }

        [Required]
        public string RoomId { get; set; }
        public Room Room { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
