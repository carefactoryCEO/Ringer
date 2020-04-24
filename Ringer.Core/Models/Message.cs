using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ringer.Core.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Sender { get; set; } // sender
        public int SenderId { get; set; }

        public Room Room { get; set; }
        [Required]
        public string RoomId { get; set; }

    }
}
