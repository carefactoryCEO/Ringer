using System;

namespace Ringer.Core.Models
{
    public class Ticket
    {
        public int ID { get; set; }

        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }
    }
}