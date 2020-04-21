using System;
namespace Ringer.Core.Models
{
    public class FootPrint
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
