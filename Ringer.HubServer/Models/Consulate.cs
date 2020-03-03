using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ringer.HubServer.Models
{
    public class Consulate
    {
        public int Id { get; set; }
        public string ConsulateType { get; set; }
        public string Country { get; set; }
        public string KoreanName { get; set; }
        public string LocalName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Homepage { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GoogleMap { get; set; }
    }
}