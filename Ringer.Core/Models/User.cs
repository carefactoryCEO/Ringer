using System;

namespace Ringer.Core.Models
{
    public class User
    {
        public string Id { get; }
        public string Name { get; }
        public DateTime BirthDate { get; }
        public Sex Sex { get; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool Paid { get; set; }
    }

    public enum Sex
    {
        Female, 
        Male
    }
}
